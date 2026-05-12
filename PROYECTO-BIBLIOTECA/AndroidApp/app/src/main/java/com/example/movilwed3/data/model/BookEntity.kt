package com.example.movilwed3.data.model

import androidx.room.Entity
import androidx.room.PrimaryKey
import com.example.movilwed3.domain.model.Book

@Entity(tableName = "books")
data class BookEntity(
    @PrimaryKey val id: Int,
    val title: String,
    val author: String,
    val imageUrl: String,
    val description: String,
    val price: Double,
    val stock: Int
)

fun BookEntity.toBook() = Book(
    id = id,
    title = title,
    author = author,
    imageUrl = imageUrl,
    description = description,
    price = price,
    stock = stock
)

fun Book.toEntity() = BookEntity(
    id = id,
    title = title,
    author = author,
    imageUrl = imageUrl,
    description = description,
    price = price,
    stock = stock
)
