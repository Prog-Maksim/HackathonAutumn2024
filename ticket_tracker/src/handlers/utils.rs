use crate::api_response::ApiError;

pub async fn hello_word() -> &'static str {
    "Hello, World!"
}

pub async fn handler_404() -> ApiError {
    ApiError::NotFound("Page not found".to_owned())
}
