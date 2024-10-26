use ticket_tracking::App;

#[tokio::main]
async fn main() {
    dotenv::dotenv().ok();

    let app = App::new().await;
    app.run().await;
}
