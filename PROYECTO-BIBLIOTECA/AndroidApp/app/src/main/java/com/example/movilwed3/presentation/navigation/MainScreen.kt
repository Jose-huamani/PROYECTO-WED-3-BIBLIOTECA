package com.example.movilwed3.presentation.navigation

import androidx.compose.foundation.layout.padding
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Book
import androidx.compose.material.icons.filled.Favorite
import androidx.compose.material.icons.filled.Home
import androidx.compose.material.icons.filled.Person
import androidx.compose.material.icons.filled.ShoppingBag
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.ui.Modifier
import androidx.navigation.NavDestination.Companion.hierarchy
import androidx.navigation.NavGraph.Companion.findStartDestination
import androidx.navigation.NavHostController
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.currentBackStackEntryAsState
import androidx.navigation.compose.rememberNavController
import com.example.movilwed3.presentation.home.HomeScreen
import com.example.movilwed3.ui.theme.CyanAccent
import com.example.movilwed3.ui.theme.GrayLight
import com.example.movilwed3.ui.theme.NavyDark
import com.example.movilwed3.ui.theme.NavyLight

sealed class BottomNavItem(val route: String, val title: String, val icon: androidx.compose.ui.graphics.vector.ImageVector) {
    object Home : BottomNavItem(Screen.Home, "Inicio", Icons.Default.Home)
    object Books : BottomNavItem(Screen.Books, "Libros", Icons.Default.Book)
    object Loans : BottomNavItem(Screen.Loans, "Préstamos", Icons.Default.ShoppingBag)
    object Favorites : BottomNavItem("favorites_screen", "Favoritos", Icons.Default.Favorite)
    object Profile : BottomNavItem("profile_screen", "Perfil", Icons.Default.Person)
}

@Composable
fun MainScreen(rootNavController: NavHostController) {
    val navController = rememberNavController()
    
    val items = listOf(
        BottomNavItem.Home,
        BottomNavItem.Books,
        BottomNavItem.Loans,
        BottomNavItem.Favorites,
        BottomNavItem.Profile
    )

    Scaffold(
        bottomBar = {
            NavigationBar(
                containerColor = NavyDark,
                contentColor = CyanAccent
            ) {
                val navBackStackEntry by navController.currentBackStackEntryAsState()
                val currentDestination = navBackStackEntry?.destination

                items.forEach { item ->
                    NavigationBarItem(
                        icon = { Icon(item.icon, contentDescription = item.title) },
                        label = { Text(item.title) },
                        selected = currentDestination?.hierarchy?.any { it.route == item.route } == true,
                        colors = NavigationBarItemDefaults.colors(
                            selectedIconColor = CyanAccent,
                            selectedTextColor = CyanAccent,
                            unselectedIconColor = GrayLight,
                            unselectedTextColor = GrayLight,
                            indicatorColor = NavyLight
                        ),
                        onClick = {
                            navController.navigate(item.route) {
                                popUpTo(navController.graph.findStartDestination().id) {
                                    saveState = true
                                }
                                launchSingleTop = true
                                restoreState = true
                            }
                        }
                    )
                }
            }
        }
    ) { innerPadding ->
        NavHost(
            navController = navController,
            startDestination = Screen.Home,
            modifier = Modifier.padding(innerPadding)
        ) {
            composable(Screen.Home) {
                HomeScreen(
                    navController = rootNavController, // Le pasamos el root para que pueda hacer Logout
                    onNavigateToBooks = { navController.navigate(Screen.Books) }
                )
            }
            composable(Screen.Books) {
                com.example.movilwed3.presentation.books.BooksScreen(navController = rootNavController)
            }
            composable(Screen.Loans) {
                com.example.movilwed3.presentation.loans.LoansScreen()
            }
            composable("favorites_screen") {
                com.example.movilwed3.presentation.favorites.FavoritesScreen(navController = rootNavController)
            }
            composable("profile_screen") {
                com.example.movilwed3.presentation.profile.ProfileScreen(navController = rootNavController)
            }
        }
    }
}
