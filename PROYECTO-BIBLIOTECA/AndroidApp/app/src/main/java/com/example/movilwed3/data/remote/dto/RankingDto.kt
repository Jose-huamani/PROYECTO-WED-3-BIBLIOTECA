package com.example.movilwed3.data.remote.dto

data class RankingDto(
    val userId: String,
    val firstName: String,
    val lastName: String,
    val points: Int,
    val position: Int,
    val profilePictureUrl: String?
)
