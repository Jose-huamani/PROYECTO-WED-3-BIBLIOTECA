package com.example.movilwed3.data.local

import androidx.room.Database
import androidx.room.RoomDatabase
import com.example.movilwed3.data.model.BookEntity

@Database(
    entities = [BookEntity::class],
    version = 1,
    exportSchema = false
)
abstract class LibraryDatabase :
    RoomDatabase() {

    abstract fun libraryDao():
            LibraryDao
}