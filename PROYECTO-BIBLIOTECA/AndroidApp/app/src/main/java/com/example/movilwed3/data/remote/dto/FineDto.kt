package com.example.movilwed3.data.remote.dto

data class FineDto(
    val id: Int,
    val amount: Double,
    val reason: String,
    val date: String,
    val isPaid: Boolean
)
