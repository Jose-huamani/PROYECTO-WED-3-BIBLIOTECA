package com.example.movilwed3.data.remote.dto

import com.google.gson.annotations.SerializedName

data class LineaPrestamoDto(
    @SerializedName("libroId") val libroId: Int,
    @SerializedName("cantidad") val cantidad: Int = 1
)

data class LoanRequest(
    @SerializedName("diasPrestamo") val diasPrestamo: Int = 14,
    @SerializedName("lineas") val lineas: List<LineaPrestamoDto>
)
