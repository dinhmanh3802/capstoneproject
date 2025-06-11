import React, { useState } from "react"

import logo from "../../assets/img/logo.png"
import coloanimg from "../../assets/img/coloan-img.jpg"
import { ForgotPassword, Login } from "../../pages"
import { Outlet } from "react-router-dom"

const AuthLayout = () => {
    return (
        <div className="container-fluid vh-100">
            <div className="row h-100">
                <div
                    className="col-md-8 d-none d-md-block"
                    style={{
                        backgroundImage: `url(${coloanimg})`,
                        backgroundSize: "cover",
                        backgroundPosition: "center",
                    }}
                ></div>

                <div className="col-md-4 col-12 d-flex align-items-center justify-content-center">
                    <div className="p-5 shadow-lg bg-white rounded" style={{ width: "100%", maxWidth: "400px" }}>
                        <div className="text-center mb-5">
                            <img src={logo} alt="Logo" style={{ width: "100px", height: "100px" }} />
                            <h2 className="mt-2 fw-bold">Chùa Cổ Loan</h2>
                        </div>

                        {/* Để hiển thị Login hoặc ForgotPassword */}
                        <Outlet />
                    </div>
                </div>
            </div>
        </div>
    )
}

export default AuthLayout
