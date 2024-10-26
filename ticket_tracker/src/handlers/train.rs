use crate::api_response::{ApiError, ApiResult};
use crate::models::train::Train;
use crate::{Id, API_KEY, ENDPOINT_URL};
use reqwest::{
    header::{HeaderMap, HeaderValue, ACCEPT, AUTHORIZATION},
    Client,
};

pub async fn get(id: Id) -> ApiResult<Train> {
    let url = format!("{}={}", &*ENDPOINT_URL, id);

    let mut headers = HeaderMap::new();
    headers.insert(ACCEPT, HeaderValue::from_static("application/json"));
    headers.insert(AUTHORIZATION, HeaderValue::from_static(&API_KEY));

    let wagon = Client::new()
        .get(url)
        .headers(headers)
        .send()
        .await
        .map_err(|_| ApiError::InternalServerError("Interval server error".into()))?
        .json::<Train>()
        .await
        .map_err(|_| ApiError::BadRequest("Validation error".into()))?;

    Ok(wagon)
}
