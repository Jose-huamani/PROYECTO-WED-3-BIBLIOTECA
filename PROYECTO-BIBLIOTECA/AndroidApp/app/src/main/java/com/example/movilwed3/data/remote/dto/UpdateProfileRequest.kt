package com.example.movilwed3.data.remote.dto

import com.google.gson.annotations.SerializedName

data class UpdateProfileRequest(
    @SerializedName("nombreCompleto") val nombreCompleto: String,
    @SerializedName("password") val password: String? = null
)
