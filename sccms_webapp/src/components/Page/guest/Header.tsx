import React, { useEffect, useState } from "react"
import { Link, useLocation } from "react-router-dom"

const Header: React.FC = () => {
    const location = useLocation()
    const [dropdowns, setDropdowns] = useState<{ [key: string]: boolean }>({
        posts: false,
        register: false,
    })
    const [isNavCollapsed, setIsNavCollapsed] = useState(true)
    const [isMobileView, setIsMobileView] = useState(window.innerWidth <= 992)

    const isHomeActive = location.pathname === "/home"
    const isPostsActive = location.pathname.startsWith("/home/category")
    const isRegisterActive =
        location.pathname === "/home/register/student" || location.pathname === "/home/register/volunteer"
    const isResultActive = location.pathname === "/home/result"
    const isIntroductionActive = location.pathname === "/home/category/0"
    const isActivitiesActive = location.pathname === "/home/category/1"
    const isGuideActive = location.pathname === "/home/category/2"
    const isStudentRegisterActive = location.pathname === "/home/register/student"
    const isVolunteerRegisterActive = location.pathname === "/home/register/volunteer"
    const isFeedbackActive = location.pathname === "/home/feedback"

    useEffect(() => {
        function handleResize() {
            const isMobile = window.innerWidth <= 992
            setIsMobileView(isMobile)
            if (!isMobile) {
                setDropdowns({ posts: false, register: false })
                setIsNavCollapsed(true)
            }
        }

        window.addEventListener("resize", handleResize)
        return () => window.removeEventListener("resize", handleResize)
    }, [])

    const toggleDropdown = (dropdownName: string) => {
        if (isMobileView) {
            setDropdowns((prevState) => ({
                posts: dropdownName === "posts" ? !prevState.posts : false,
                register: dropdownName === "register" ? !prevState.register : false,
            }))
        }
    }

    const handleMouseOver = (dropdownName: string) => {
        if (!isMobileView) {
            setDropdowns((prevState) => ({
                ...prevState,
                [dropdownName]: true,
            }))
        }
    }

    const handleMouseOut = (dropdownName: string) => {
        if (!isMobileView) {
            setDropdowns((prevState) => ({
                ...prevState,
                [dropdownName]: false,
            }))
        }
    }

    const handleDropdownItemClick = () => {
        setDropdowns({ posts: false, register: false })
        setIsNavCollapsed(true)
    }

    return (
        <div className="container-fluid p-0">
            <nav className="navbar navbar-expand-lg bg-dark navbar-dark py-2 py-lg-0 px-lg-5 fixed-top">
                <button className="navbar-toggler" type="button" onClick={() => setIsNavCollapsed(!isNavCollapsed)}>
                    <span className="navbar-toggler-icon"></span>
                </button>
                <div
                    className={`collapse navbar-collapse justify-content-between px-0 px-lg-3 ${
                        isNavCollapsed ? "" : "show"
                    }`}
                    id="navbarCollapse"
                >
                    <div className="navbar-nav mr-auto py-0">
                        <Link to="/home" className={`nav-item nav-link ${isHomeActive ? "active" : ""}`}>
                            Trang chủ
                        </Link>
                        <div
                            className="nav-item dropdown"
                            onMouseOver={() => handleMouseOver("posts")}
                            onMouseOut={() => handleMouseOut("posts")}
                            onClick={() => toggleDropdown("posts")}
                        >
                            <div
                                className={`nav-link dropdown-toggle ${isPostsActive ? "active" : ""}`}
                                style={{ cursor: "pointer" }}
                            >
                                Bài viết
                            </div>
                            <div className={`dropdown-menu rounded-0 m-0 ${dropdowns.posts ? "show" : ""}`}>
                                <Link
                                    to="/home/category/0"
                                    className={`dropdown-item ${isIntroductionActive ? "active" : ""}`}
                                    onClick={handleDropdownItemClick}
                                >
                                    Giới thiệu
                                </Link>
                                <Link
                                    to="/home/category/1"
                                    className={`dropdown-item ${isActivitiesActive ? "active" : ""}`}
                                    onClick={handleDropdownItemClick}
                                >
                                    Hoạt động khoá tu
                                </Link>
                                <Link
                                    to="/home/category/2"
                                    className={`dropdown-item ${isGuideActive ? "active" : ""}`}
                                    onClick={handleDropdownItemClick}
                                >
                                    Hướng dẫn đăng kí
                                </Link>
                            </div>
                        </div>
                        <div
                            className="nav-item dropdown"
                            onMouseOver={() => handleMouseOver("register")}
                            onMouseOut={() => handleMouseOut("register")}
                            onClick={() => toggleDropdown("register")}
                        >
                            <div
                                className={`nav-link dropdown-toggle ${isRegisterActive ? "active" : ""}`}
                                style={{ cursor: "pointer" }}
                            >
                                Đăng kí
                            </div>
                            <div className={`dropdown-menu rounded-0 m-0 ${dropdowns.register ? "show" : ""}`}>
                                <Link
                                    to="/home/register/student"
                                    className={`dropdown-item ${isStudentRegisterActive ? "active" : ""}`}
                                    onClick={handleDropdownItemClick}
                                >
                                    Đăng kí khoá sinh
                                </Link>
                                <Link
                                    to="/home/register/volunteer"
                                    className={`dropdown-item ${isVolunteerRegisterActive ? "active" : ""}`}
                                    onClick={handleDropdownItemClick}
                                >
                                    Đăng kí tình nguyện viên
                                </Link>
                            </div>
                        </div>
                        <Link to="/home/result" className={`nav-item nav-link ${isResultActive ? "active" : ""}`}>
                            Kết quả học
                        </Link>
                        <Link to="/home/feedback" className={`nav-item nav-link ${isFeedbackActive ? "active" : ""}`}>
                            Phản hồi
                        </Link>
                    </div>
                </div>
            </nav>
        </div>
    )
}

export default Header
