import React from "react"
import "../assets/css/homePage.css"
import "@fortawesome/fontawesome-free/css/all.min.css"
import { Header, Footer } from "../components/Page/guest"
import { Outlet } from "react-router-dom"
function HomePage() {
    return (
        <div>
            <Header />
            <Outlet />
            <Footer />
        </div>
    )
}

export default HomePage
