package com.example.movilwed3.data.remote.dto

import com.example.movilwed3.domain.model.Book
import com.google.gson.annotations.SerializedName

data class BookDto(
    @SerializedName("id") val id: Int,
    @SerializedName("titulo") val titulo: String?,
    @SerializedName("isbn") val isbn: String?,
    @SerializedName("cantidadDisponible") val cantidadDisponible: Int?,
    @SerializedName("estaDisponible") val estaDisponible: Boolean?,
    @SerializedName("imagenUrl") val imagenUrl: String?,
    @SerializedName("precio") val precio: Double?,
    @SerializedName("autor") val autor: AuthorDto?,
    @SerializedName("categoria") val categoria: CategoryDto?
)

fun BookDto.toBook(): Book {
    return Book(
        id = id,
        title = titulo ?: "Sin título",
        author = autor?.nombre ?: "Autor desconocido",
        imageUrl = imagenUrl ?: "",
        description = "ISBN: ${isbn ?: "N/A"}",
        category = categoria?.nombre ?: "General",
        isAvailable = estaDisponible ?: false,
        price = precio ?: 0.0,
        stock = cantidadDisponible ?: 0
    )
}
