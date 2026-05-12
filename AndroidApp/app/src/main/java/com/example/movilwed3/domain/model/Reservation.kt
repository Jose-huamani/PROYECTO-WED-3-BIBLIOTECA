package com.example.movilwed3.domain.model

data class Reservation(
    val id: Int,
    val bookTitle: String,
    val bookAuthor: String,
    val reservationDate: String,
    val expirationDate: String,
    val status: String
)
