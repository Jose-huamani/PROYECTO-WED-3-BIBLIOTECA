package com.example.movilwed3.di

import android.content.Context
import androidx.room.Room
import com.example.movilwed3.data.local.LibraryDao
import com.example.movilwed3.data.local.LibraryDatabase
import dagger.Module
import dagger.Provides
import dagger.hilt.InstallIn
import dagger.hilt.android.qualifiers.ApplicationContext
import dagger.hilt.components.SingletonComponent
import javax.inject.Singleton

@Module
@InstallIn(SingletonComponent::class)
object DatabaseModule {

    @Provides
    @Singleton
    fun provideDatabase(@ApplicationContext context: Context): LibraryDatabase {
        return Room.databaseBuilder(
            context,
            LibraryDatabase::class.java,
            "library_db"
        ).build()
    }

    @Provides
    @Singleton
    fun provideLibraryDao(db: LibraryDatabase): LibraryDao {
        return db.libraryDao()
    }
}
