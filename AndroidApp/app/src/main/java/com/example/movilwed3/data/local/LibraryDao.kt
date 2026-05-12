package com.example.movilwed3.data.local

import androidx.room.Dao
import androidx.room.Insert
import androidx.room.OnConflictStrategy
import androidx.room.Query
import com.example.movilwed3.data.model.BookEntity

@Dao
interface LibraryDao {

    @Query("SELECT * FROM books")
    suspend fun getAllBooks():
            List<BookEntity>

    @Insert(
        onConflict =
            OnConflictStrategy.REPLACE
    )
    suspend fun insertBooks(
        books: List<BookEntity>
    )
}