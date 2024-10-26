use crate::Id;
use serde::Deserialize;

pub type Train = Vec<Wagon>;

#[derive(Debug, Deserialize)]
pub struct Wagon {
    pub seats: Vec<Seat>,
    #[serde(rename = "type")]
    pub wagon_type: WagonType,
    pub wagon_id: Id,
}

#[derive(Debug, Deserialize)]
pub struct Seat {
    block: String,
    #[serde(rename = "bookingStatus")]
    pub booking_status: BookingStatus,
    price: u32,
    #[serde(rename = "seatNum")]
    seat_num: String,
    seat_id: Id,
}

#[derive(Debug, Eq, Deserialize, PartialEq)]
#[serde(rename_all = "UPPERCASE")]
pub enum BookingStatus {
    Free,
    Booked,
}

#[derive(Debug, PartialEq, Eq, Hash, Clone, Deserialize)]
#[serde(rename_all = "UPPERCASE")]
pub enum WagonType {
    Platzcart,
    Couple,
    Luxury,
}

#[derive(Debug, PartialEq, Eq, Hash, Clone, Deserialize)]
pub struct TakePlace {
    pub train_id: Id,
    pub wagon_type: WagonType,
}
