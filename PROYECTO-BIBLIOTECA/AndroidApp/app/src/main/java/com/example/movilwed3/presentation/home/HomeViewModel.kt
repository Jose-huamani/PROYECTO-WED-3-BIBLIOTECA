package com.example.movilwed3.presentation.home

import androidx.compose.runtime.State
import androidx.compose.runtime.mutableStateOf
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.movilwed3.data.remote.dto.MultaDto
import com.example.movilwed3.data.remote.dto.LoanDto
import com.example.movilwed3.domain.repository.LibraryRepository
import com.example.movilwed3.utils.SessionManager
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.launch
import javax.inject.Inject

@HiltViewModel
class HomeViewModel @Inject constructor(
    private val repository: LibraryRepository,
    private val sessionManager: SessionManager
) : ViewModel() {

    private val _prestamos = mutableStateOf<List<LoanDto>>(emptyList())
    val prestamos: State<List<LoanDto>> = _prestamos

    private val _multas = mutableStateOf<List<MultaDto>>(emptyList())
    val multas: State<List<MultaDto>> = _multas

    private val _isLoading = mutableStateOf(false)
    val isLoading: State<Boolean> = _isLoading

    init {
        loadDashboardData()
    }

    fun loadDashboardData() {
        viewModelScope.launch {
            _isLoading.value = true
            
            val prestamosResult = repository.getMisPrestamos()
            val multasResult = repository.getMisMultas()

            prestamosResult.onSuccess { list ->
                _prestamos.value = list.filter { it.statusInt == 1 } // Solo activos
            }
            
            multasResult.onSuccess { list ->
                _multas.value = list.filter { !it.pagada } // Solo pendientes
            }

            _isLoading.value = false
        }
    }

    fun logout(onSuccess: () -> Unit) {
        viewModelScope.launch {
            sessionManager.clearSession()
            onSuccess()
        }
    }
}
