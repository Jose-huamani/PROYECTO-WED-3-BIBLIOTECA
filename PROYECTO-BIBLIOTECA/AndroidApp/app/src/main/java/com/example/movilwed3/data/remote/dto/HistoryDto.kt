package com.example.movilwed3.data.remote.dto

import com.google.gson.annotations.SerializedName

data class HistoryDto(
    @SerializedName("id") val id: Int,
    @SerializedName("usuarioId") val usuarioId: Int,
    @SerializedName("tipoEvento") val tipoEvento: String,
    @SerializedName("descripcion") val descripcion: String,
    @SerializedName("libroId") val libroId: Int?,
    @SerializedName("prestamoId") val prestamoId: Int?,
    @SerializedName("fecha") val fecha: String
)
