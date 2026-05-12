package com.example.movilwed3.data.repository

import com.example.movilwed3.data.remote.api.LibraryApi
import com.example.movilwed3.data.remote.dto.*
import com.example.movilwed3.domain.repository.LibraryRepository
import com.example.movilwed3.utils.SessionManager
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
class LibraryRepositoryImpl @Inject constructor(
    private val api: LibraryApi,
    private val sessionManager: SessionManager
) : LibraryRepository {

    private suspend fun <T> safeApiCall(apiCall: suspend () -> retrofit2.Response<T>): Result<T> {
        return try {
            val response = apiCall()
            if (response.isSuccessful) {
                val body = response.body()
                if (body != null) {
                    Result.success(body)
                } else {
                    @Suppress("UNCHECKED_CAST")
                    Result.success(Unit as T)
                }
            } else {
                val errorMsg = when (response.code()) {
                    400 -> "Solicitud incorrecta (400)"
                    401 -> "Credenciales inválidas o sesión expirada"
                    403 -> "No tienes permisos (403)"
                    404 -> "Recurso no encontrado (404)"
                    409 -> "Conflicto: El registro ya existe"
                    500 -> "Error interno del servidor (500)"
                    else -> "Error del servidor: ${response.code()}"
                }
                Result.failure(Exception(errorMsg))
            }
        } catch (e: java.net.ConnectException) {
            Result.failure(Exception("Error de conexión: Verifica que el servidor esté encendido."))
        } catch (e: java.net.SocketTimeoutException) {
            Result.failure(Exception("Tiempo de espera agotado: El servidor tardó mucho en responder."))
        } catch (e: java.net.UnknownHostException) {
            Result.failure(Exception("No se encuentra el servidor. Verifica tu conexión a internet o IP."))
        } catch (e: javax.net.ssl.SSLHandshakeException) {
            Result.failure(Exception("Error de seguridad SSL al conectar con el servidor."))
        } catch (e: Exception) {
            Result.failure(Exception("Ocurrió un error inesperado: ${e.localizedMessage}"))
        }
    }

    override suspend fun login(request: LoginRequest): Result<AuthResponse> {
        val result = safeApiCall { api.login(request) }
        result.onSuccess { response ->
            sessionManager.saveAuthTokens(response.accessToken, response.refreshToken)
        }
        return result
    }

    override suspend fun register(request: RegisterRequest): Result<AuthResponse> {
        val result = safeApiCall { api.register(request) }
        result.onSuccess { response ->
            sessionManager.saveAuthTokens(response.accessToken, response.refreshToken)
        }
        return result
    }

    override suspend fun getProfile() = safeApiCall { api.getProfile() }

    override suspend fun updateProfile(request: UpdateProfileRequest) = safeApiCall { api.updateProfile(request) }

    override suspend fun getLibros() = safeApiCall { api.getLibros() }
    
    override suspend fun getLibro(id: Int) = safeApiCall { api.getLibro(id) }

    override suspend fun getFavoritos() = safeApiCall { api.getFavoritos() }
    
    override suspend fun addFavorito(libroId: Int) = safeApiCall { api.addFavorito(libroId) }

    override suspend fun solicitarPrestamo(libroId: Int, dias: Int) = safeApiCall { 
        api.solicitarPrestamo(PrestamoCreateRequest(diasPrestamo = dias, lineas = listOf(LineaPrestamoDto(libroId, 1)))) 
    }

    override suspend fun solicitarReserva(libroId: Int) = safeApiCall {
        api.solicitarReserva(libroId)
    }

    override suspend fun getMisReservas() = safeApiCall { api.getMisReservas() }

    override suspend fun getMisPrestamos() = safeApiCall { api.getMisPrestamos() }

    override suspend fun getMisMultas() = safeApiCall { api.getMisMultas() }

    override suspend fun pagarMultaQR(multaId: Int, comprobante: String) = safeApiCall {
        api.pagarMultaQR(PagoQrRequest(multaId, comprobante))
    }

    override suspend fun registerFcmToken(token: String) = safeApiCall {
        api.registerFcmToken(FcmTokenRequest(token, null))
    }

    override suspend fun getNotificaciones() = safeApiCall { api.getNotificaciones() }

    override suspend fun getCarrito() = safeApiCall { api.getCarrito() }

    override suspend fun addCarritoItem(libroId: Int, cantidad: Int) = safeApiCall {
        api.addCarritoItem(libroId, cantidad)
    }

    override suspend fun checkoutCarrito(metodoPagoId: Int, comprobante: String?) = safeApiCall {
        api.checkoutCarrito(VentaCreateRequest(metodoPagoId, comprobante))
    }
}
