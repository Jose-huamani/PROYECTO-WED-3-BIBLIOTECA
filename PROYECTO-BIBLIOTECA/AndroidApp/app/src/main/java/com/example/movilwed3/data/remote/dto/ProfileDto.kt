package com.example.movilwed3.data.remote.dto

import com.google.gson.annotations.SerializedName

data class ProfileDto(
    @SerializedName("id") val id: Int,
    @SerializedName("nombreCompleto") val nombreCompleto: String,
    @SerializedName("email") val email: String,
    @SerializedName("rol") val rol: String,
    @SerializedName("activo") val activo: Boolean,
    @SerializedName("puntos") val puntos: Int
)
