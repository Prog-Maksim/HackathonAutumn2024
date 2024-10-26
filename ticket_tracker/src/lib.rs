mod api_response;
mod handlers;
mod models;

use crate::handlers::utils::{hello_word, handler_404};
use crate::models::queue::Queue;
use axum::{routing, Router};
use handlers::queue;
use std::sync::{Arc, LazyLock};
use tokio::net::TcpListener;

pub type Id = u16;
static API_KEY: LazyLock<String> = LazyLock::new(|| std::env::var("API_KEY").unwrap());
static ENDPOINT_URL: LazyLock<String> = LazyLock::new(|| std::env::var("ENDPOINT_URL").unwrap());

struct AppState {
    queue: Queue,
}

impl AppState {
    fn new() -> Self {
        AppState {
            queue: Queue::new(),
        }
    }
}

pub struct App {
    listener: TcpListener,
    router: Router,
}

impl App {
    pub async fn new() -> Self {
        let listener = Self::init_tcp_listener().await;
        let router = Self::init_router().await;

        App { listener, router }
    }

    fn init_state() -> Arc<AppState> {
        Arc::new(AppState::new())
    }

    async fn init_tcp_listener() -> TcpListener {
        let host = std::env::var("HOST").unwrap_or_else(|_| "127.0.0.1".into());
        let port = std::env::var("PORT").unwrap_or_else(|_| "3000".into());
        let addr = format!("{}:{}", host, port);

        TcpListener::bind(addr).await.unwrap()
    }

    async fn init_router() -> Router {
        let state = Self::init_state();
        Self::start_background_task(state.clone()).await;

        Router::new()
            .route("/", routing::get(hello_word))
            .route("/queue", routing::post(queue::add))
            .fallback(handler_404)
            .with_state(state)
    }

    async fn start_background_task(state: Arc<AppState>) {
        tokio::spawn(queue::check_update(state));
    }

    pub async fn run(self) {
        axum::serve(self.listener, self.router).await.unwrap();
    }
}
