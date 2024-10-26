use super::train;
use crate::api_response::ApiResult;
use crate::models::train::TakePlace;
use crate::{AppState, Id};
use axum::{
    extract::{Path, State},
    http::StatusCode,
    Json,
};
use std::sync::Arc;
use tokio::time::{self, Duration};

pub async fn add(
    Path(user_id): Path<Id>,
    State(state): State<Arc<AppState>>,
    Json(place): Json<TakePlace>,
) -> ApiResult<StatusCode> {
    state.queue.add(place, user_id);
    Ok(StatusCode::CREATED)
}

pub async fn check_update(state: Arc<AppState>) -> Result<(), ()> {
    loop {
        for take_place in state.queue.get_keys() {
            let train = train::get(take_place.train_id).await.map_err(|_| ())?;

            state.queue.process_reservations(train, take_place).await;
        }
        time::sleep(Duration::from_secs(5)).await;
    }
}
