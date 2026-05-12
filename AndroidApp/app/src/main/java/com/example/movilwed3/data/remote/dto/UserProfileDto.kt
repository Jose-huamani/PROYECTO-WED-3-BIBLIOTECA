package com.example.movilwed3.data.remote.dto

import com.google.gson.annotations.SerializedName

data class UserProfileDto(
    @SerializedName("id") val id: String,
    @SerializedName("email") val email: String,
    @SerializedName("userName") val userName: String?,
    @SerializedName("firstName") val firstName: String?,
    @SerializedName("lastName") val lastName: String?,
    @SerializedName("profilePictureUrl") val profilePictureUrl: String?,
    @SerializedName("rol") val role: String?
)
