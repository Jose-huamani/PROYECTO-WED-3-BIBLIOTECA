package com.example.movilwed3.data.remote.dto

import com.example.movilwed3.domain.model.Loan

import com.google.gson.annotations.SerializedName

data class LoanDto(
    @SerializedName("id") val id: Int,
    @SerializedName("detalles") val detalles: List<DetallePrestamoDto>?,
    @SerializedName("fechaPrestamo") val loanDate: String?,
    @SerializedName("fechaDevolucionEsperada") val dueDate: String?,
    @SerializedName("fechaDevolucionReal") val returnDate: String?,
    @SerializedName("estado") val statusInt: Int?
)

data class DetallePrestamoDto(
    @SerializedName("libro") val libro: BookDto?
)

fun LoanDto.toLoan(): Loan {
    val firstBook = detalles?.firstOrNull()?.libro
    return Loan(
        id = id,
        bookTitle = firstBook?.titulo ?: "Libro desconocido",
        bookAuthor = firstBook?.autor?.nombre ?: "Autor desconocido",
        loanDate = loanDate ?: "",
        dueDate = dueDate ?: "",
        returnDate = returnDate,
        status = when(statusInt) {
            1 -> "Activo"
            2 -> "Devuelto"
            3 -> "Vencido"
            4 -> "Cancelado"
            else -> "Desconocido"
        }
    )
}
