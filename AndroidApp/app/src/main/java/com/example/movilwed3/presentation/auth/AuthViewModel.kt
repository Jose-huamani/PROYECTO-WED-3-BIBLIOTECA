package com.example.movilwed3.presentation.auth

import androidx.compose.runtime.State
import androidx.compose.runtime.mutableStateOf
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.movilwed3.data.remote.dto.LoginRequest
import com.example.movilwed3.domain.repository.LibraryRepository
import com.example.movilwed3.utils.SessionManager
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.firstOrNull
import kotlinx.coroutines.launch
import javax.inject.Inject

@HiltViewModel
class AuthViewModel @Inject constructor(
    private val repository: LibraryRepository,
    private val sessionManager: SessionManager
) : ViewModel() {

    private val _email = mutableStateOf("")
    val email: State<String> = _email

    private val _password = mutableStateOf("")
    val password: State<String> = _password

    private val _isLoading = mutableStateOf(false)
    val isLoading: State<Boolean> = _isLoading

    private val _error = mutableStateOf<String?>(null)
    val error: State<String?> = _error

    private val _loginSuccess = mutableStateOf(false)
    val loginSuccess: State<Boolean> = _loginSuccess

    fun onEmailChange(newEmail: String) {
        _email.value = newEmail
        _error.value = null
    }

    fun onPasswordChange(newPassword: String) {
        _password.value = newPassword
        _error.value = null
    }

    fun login() {
        if (_email.value.isBlank() || _password.value.isBlank()) {
            _error.value = "Por favor, completa todos los campos"
            return
        }

        viewModelScope.launch {
            _isLoading.value = true
            _error.value = null

            val result = repository.login(LoginRequest(_email.value, _password.value))
            
            result.onSuccess {
                // Notificar FCM al backend después de login
                val token = sessionManager.fcmToken.firstOrNull()
                if (!token.isNullOrEmpty()) {
                    repository.registerFcmToken(token)
                }
                
                _isLoading.value = false
                _loginSuccess.value = true
            }.onFailure { exception ->
                _isLoading.value = false
                _error.value = exception.message ?: "Error al iniciar sesión"
            }
        }
    }
    
    private val _nombreCompleto = mutableStateOf("")
    val nombreCompleto: State<String> = _nombreCompleto

    fun onNombreChange(newNombre: String) {
        _nombreCompleto.value = newNombre
        _error.value = null
    }

    fun register() {
        if (_email.value.isBlank() || _password.value.isBlank() || _nombreCompleto.value.isBlank()) {
            _error.value = "Por favor, completa todos los campos"
            return
        }

        viewModelScope.launch {
            _isLoading.value = true
            _error.value = null

            val result = repository.register(com.example.movilwed3.data.remote.dto.RegisterRequest(_nombreCompleto.value, _email.value, _password.value))
            
            result.onSuccess {
                // Notificar FCM al backend después de registro
                val token = sessionManager.fcmToken.firstOrNull()
                if (!token.isNullOrEmpty()) {
                    repository.registerFcmToken(token)
                }
                
                _isLoading.value = false
                _loginSuccess.value = true
            }.onFailure { exception ->
                _isLoading.value = false
                _error.value = exception.message ?: "Error al registrar cuenta"
            }
        }
    }
}
