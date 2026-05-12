package com.example.movilwed3.presentation.profile

import androidx.compose.runtime.State
import androidx.compose.runtime.mutableStateOf
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.movilwed3.data.remote.dto.UpdateProfileRequest
import com.example.movilwed3.data.remote.dto.UsuarioDto
import com.example.movilwed3.domain.repository.LibraryRepository
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.launch
import javax.inject.Inject

@HiltViewModel
class ProfileViewModel @Inject constructor(
    private val repository: LibraryRepository
) : ViewModel() {

    private val _profile = mutableStateOf<UsuarioDto?>(null)
    val profile: State<UsuarioDto?> = _profile

    private val _isLoading = mutableStateOf(false)
    val isLoading: State<Boolean> = _isLoading

    private val _message = mutableStateOf<String?>(null)
    val message: State<String?> = _message

    init {
        loadProfile()
    }

    private fun loadProfile() {
        viewModelScope.launch {
            _isLoading.value = true
            val result = repository.getProfile()
            result.onSuccess {
                _profile.value = it
            }.onFailure {
                _message.value = "Error al cargar perfil: ${it.message}"
            }
            _isLoading.value = false
        }
    }

    fun updateProfile(nombreCompleto: String, password: String?) {
        viewModelScope.launch {
            _isLoading.value = true
            val request = UpdateProfileRequest(
                nombreCompleto = nombreCompleto,
                password = if (password.isNullOrBlank()) null else password
            )
            val result = repository.updateProfile(request)
            
            result.onSuccess {
                _message.value = "Perfil actualizado exitosamente."
                loadProfile() // Recargar los nuevos datos
            }.onFailure {
                _message.value = "Error al actualizar: ${it.message}"
            }
            _isLoading.value = false
        }
    }

    fun clearMessage() {
        _message.value = null
    }
}
