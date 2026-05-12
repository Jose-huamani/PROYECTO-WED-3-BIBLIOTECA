package com.example.movilwed3.domain.model

data class Loan(
    val id: Int,
    val bookTitle: String,
    val bookAuthor: String,
    val loanDate: String,
    val dueDate: String,
    val returnDate: String?,
    val status: String,
    val fineAmount: Double = 0.0
)
