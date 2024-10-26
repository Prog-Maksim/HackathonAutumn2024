use axum::{
    http::StatusCode,
    response::{IntoResponse, Response},
    Json,
};
use serde_json::json;

pub type ApiResult<T> = Result<T, ApiError>;

#[derive(Debug)]
pub enum ApiError {
    /// Error 400
    BadRequest(String),
    /// Error 403
    Forbidden(String),
    /// Error 404
    NotFound(String),
    /// Error 408
    RequestTimeout,
    /// Error 500
    InternalServerError(String),
}

impl IntoResponse for ApiError {
    fn into_response(self) -> Response {
        let (status, error_message) = match self {
            Self::BadRequest(err) => (StatusCode::BAD_REQUEST, err),
            Self::Forbidden(err) => (StatusCode::FORBIDDEN, err),
            Self::NotFound(err) => (StatusCode::NOT_FOUND, err),
            Self::RequestTimeout => (StatusCode::REQUEST_TIMEOUT, "Request timeout".to_owned()),
            Self::InternalServerError(err) => (StatusCode::INTERNAL_SERVER_ERROR, err),
        };

        let body = Json(json!({"error": error_message}));
        (status, body.to_owned()).into_response()
    }
}
