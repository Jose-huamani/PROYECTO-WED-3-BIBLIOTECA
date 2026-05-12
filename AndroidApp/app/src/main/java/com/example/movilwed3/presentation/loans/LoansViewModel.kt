package com.example.movilwed3.presentation.loans

import androidx.compose.runtime.State
import androidx.compose.runtime.mutableStateOf
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.movilwed3.data.remote.dto.LoanDto
import com.example.movilwed3.domain.repository.LibraryRepository
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.launch
import javax.inject.Inject

@HiltViewModel
class LoansViewModel @Inject constructor(
    private val repository: LibraryRepository
) : ViewModel() {

    private val _prestamos = mutableStateOf<List<LoanDto>>(emptyList())
    val prestamos: State<List<LoanDto>> = _prestamos

    private val _isLoading = mutableStateOf(false)
    val isLoading: State<Boolean> = _isLoading

    init {
        loadLoans()
    }

    fun loadLoans() {
        viewModelScope.launch {
            _isLoading.value = true
            val result = repository.getMisPrestamos()
            result.onSuccess {
                _prestamos.value = it
            }
            _isLoading.value = false
        }
    }
}
