package com.example.movilwed3.domain.model

data class Book(
    val id: Int,
    val title: String,
    val author: String,
    val imageUrl: String,
    val description: String,
    val category: String = "General",
    val isAvailable: Boolean = true,
    val price: Double = 0.0,
    val stock: Int = 0
)
