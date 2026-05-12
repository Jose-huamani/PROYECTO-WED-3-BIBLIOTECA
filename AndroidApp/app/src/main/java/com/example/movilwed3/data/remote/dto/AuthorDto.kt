package com.example.movilwed3.data.remote.dto

import com.google.gson.annotations.SerializedName

data class AuthorDto(
    @SerializedName("nombre") val nombre: String?
)
