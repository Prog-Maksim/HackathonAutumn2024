use super::train::{BookingStatus, TakePlace, Train};
use crate::Id;
use dashmap::DashMap;
use std::collections::VecDeque;

#[derive(Debug)]
pub struct Queue {
    queue: DashMap<TakePlace, VecDeque<Id>>,
}

impl Queue {
    pub fn new() -> Self {
        Self {
            queue: DashMap::new(),
        }
    }

    pub fn add(&self, take_place: TakePlace, user_id: Id) {
        if let Some(mut queue) = self.queue.get_mut(&take_place) {
            queue.push_back(user_id);
        } else {
            self.queue.insert(take_place, VecDeque::from(vec![user_id]));
        }
    }

    pub fn get_keys(&self) -> Vec<TakePlace> {
        self.queue.iter().map(|entry| entry.key().clone()).collect()
    }

    pub fn process_reservations(&self, mut train: Train, place: TakePlace) {
        let mut user_queue = self.queue.get_mut(&place).unwrap();

        for wagon in train
            .iter_mut()
            .filter(|w| w.wagon_type == place.wagon_type)
        {
            for seat in &mut wagon.seats {
                if seat.booking_status == BookingStatus::Free {
                    if let Some(user_id) = user_queue.pop_front() {
                        seat.booking_status = BookingStatus::Booked;
                        println!(
                            "User {} assigned to train {}, wagon {:?}",
                            user_id, place.train_id, place.wagon_type
                        );
                    }
                }
                if user_queue.is_empty() {
                    break;
                }
            }
        }
    }
}
