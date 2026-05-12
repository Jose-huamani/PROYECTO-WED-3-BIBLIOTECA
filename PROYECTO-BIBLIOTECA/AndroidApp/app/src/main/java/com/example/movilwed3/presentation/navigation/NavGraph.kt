package com.example.movilwed3.presentation.navigation

import androidx.compose.runtime.Composable
import androidx.navigation.NavHostController
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import com.example.movilwed3.presentation.auth.LoginScreen
import com.example.movilwed3.presentation.home.HomeScreen

object Screen {
    const val Login = "login_screen"
    const val Home = "home_screen"
    const val Register = "register_screen"
    const val Main = "main_screen"
    const val Books = "books_screen"
    const val Loans = "loans_screen"
    const val Fines = "fines_screen"
    const val Notifications = "notifications_screen"
    const val Cart = "cart_screen"
    const val QrScanner = "qr_scanner_screen"
    const val Reservations = "reservations_screen"
    const val PaymentQr = "payment_qr_screen"
    const val Settings = "settings_screen"
}

@Composable
fun AppNavGraph(
    navController: NavHostController,
    startDestination: String = Screen.Login
) {
    NavHost(
        navController = navController,
        startDestination = startDestination
    ) {
        composable(Screen.Login) {
            LoginScreen(
                onNavigateToHome = {
                    navController.navigate(Screen.Main) {
                        popUpTo(Screen.Login) { inclusive = true }
                    }
                },
                onNavigateToRegister = {
                    navController.navigate(Screen.Register)
                }
            )
        }
        
        composable(Screen.Register) {
            com.example.movilwed3.presentation.auth.RegisterScreen(
                onNavigateToHome = {
                    navController.navigate(Screen.Main) {
                        popUpTo(Screen.Register) { inclusive = true }
                        popUpTo(Screen.Login) { inclusive = true }
                    }
                },
                onNavigateToLogin = {
                    navController.popBackStack()
                }
            )
        }
        
        composable(Screen.Main) {
            MainScreen(rootNavController = navController)
        }
        
        composable(
            route = "book_detail/{bookId}",
            arguments = listOf(androidx.navigation.navArgument("bookId") { type = androidx.navigation.NavType.IntType })
        ) {
            com.example.movilwed3.presentation.books.BookDetailScreen(navController = navController)
        }

        composable(Screen.Fines) {
            com.example.movilwed3.presentation.fines.FinesScreen(navController = navController)
        }

        composable(Screen.Notifications) {
            com.example.movilwed3.presentation.notifications.NotificationsScreen(navController = navController)
        }

        composable(Screen.Cart) {
            com.example.movilwed3.presentation.store.CartScreen(navController = navController)
        }

        composable(Screen.QrScanner) {
            com.example.movilwed3.presentation.qr.QrScannerScreen(navController = navController)
        }

        composable(Screen.Reservations) {
            com.example.movilwed3.presentation.reservations.ReservationsScreen(navController = navController)
        }

        composable(Screen.Settings) {
            com.example.movilwed3.presentation.settings.SettingsScreen(navController = navController)
        }

        composable(
            route = "${Screen.PaymentQr}/{monto}/{multaId}",
            arguments = listOf(
                androidx.navigation.navArgument("monto") { type = androidx.navigation.NavType.FloatType },
                androidx.navigation.navArgument("multaId") { type = androidx.navigation.NavType.IntType }
            )
        ) { backStackEntry ->
            val monto = backStackEntry.arguments?.getFloat("monto")?.toDouble() ?: 0.0
            val multaId = backStackEntry.arguments?.getInt("multaId") ?: 0
            com.example.movilwed3.presentation.payments.PaymentQrScreen(
                navController = navController,
                monto = monto,
                multaId = multaId
            )
        }
    }
}
