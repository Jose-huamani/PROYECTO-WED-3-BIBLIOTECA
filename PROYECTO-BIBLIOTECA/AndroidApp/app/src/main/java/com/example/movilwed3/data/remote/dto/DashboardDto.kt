package com.example.movilwed3.data.remote.dto

import com.google.gson.annotations.SerializedName

data class DashboardDto(
    @SerializedName("prestamosActivos") val prestamosActivos: Int,
    @SerializedName("reservasActivas") val reservasActivas: Int,
    @SerializedName("puntosAcumulados") val puntosAcumulados: Int,
    @SerializedName("multasPendientes") val multasPendientes: Double,
    @SerializedName("librosLeidos") val librosLeidos: Int
)
