// Header.tsx
import { formatDistanceToNow } from "date-fns"
import { vi } from "date-fns/locale"
import "simplebar-react/dist/simplebar.min.css"
import SimpleBar from "simplebar-react"
import { useState, useEffect } from "react"
import { useDispatch, useSelector } from "react-redux"
import { Link, NavLink, useNavigate } from "react-router-dom"
import { authModel } from "../../interfaces"
import { logout } from "../../store/slice/authSlice"
import { RootState } from "../../store/store"
import logo from "../../assets/img/logo.png"
import {
    useGetNotificationsQuery,
    useMarkAsReadMutation,
    useMarkAllAsReadMutation,
    notificationApi,
} from "../../apis/notificationApi"
import notificationService from "../../helper/notificationService"
import { SD_Role_Name, SD_Role_Name_VN } from "../../utility/SD"

function getRoleNameVN(role) {
    const roleKey = Object.keys(SD_Role_Name).find((key) => SD_Role_Name[key] === role)
    return roleKey ? SD_Role_Name_VN[roleKey] : null
}
function Header() {
    const [isActive, setIsActive] = useState(false)
    const [isAuthChecked, setIsAuthChecked] = useState(false)
    const dispatch = useDispatch()
    const navigate = useNavigate()

    const userData: authModel | null = useSelector((state: RootState) => state.auth.user)
    const { data: notifications } = useGetNotificationsQuery(userData?.userId || 0, { skip: !userData })
    const [markAsRead] = useMarkAsReadMutation()
    const [markAllAsRead] = useMarkAllAsReadMutation()

    // Khởi tạo kết nối SignalR khi userData thay đổi
    useEffect(() => {
        let isMounted = true

        const initiateConnection = async () => {
            if (userData) {
                try {
                    await notificationService.startConnection()
                    if (isMounted) {
                        notificationService.onReceiveNotification((message, link) => {
                            // Invalidate tag để refetch notifications từ server
                            dispatch(notificationApi.util.invalidateTags(["Notification"]))
                        })
                    }
                } catch (error) {
                    console.error("SignalR Connection Error: ", error)
                }
            }
        }

        initiateConnection()

        // Cleanup khi component unmount hoặc userData thay đổi
        return () => {
            isMounted = false
            notificationService.stopConnection()
        }
    }, [userData, dispatch])

    useEffect(() => {
        if (userData && userData.userId !== undefined) {
            setIsAuthChecked(true)
        } else {
            setIsAuthChecked(false)
        }
    }, [userData])

    const handleLogout = () => {
        dispatch(logout())
        navigate("/auth/login")
    }

    const toggleClass = () => {
        setIsActive(!isActive)
        if (!isActive) {
            document.body.classList.add("toggle-sidebar")
        } else {
            document.body.classList.remove("toggle-sidebar")
        }
    }

    const handleNotificationClick = async (notificationId: number, link: string) => {
        try {
            await markAsRead(notificationId).unwrap()
            navigate(link)
        } catch (error) {
            console.error("Error marking notification as read: ", error)
        }
    }

    const handleMarkAllAsRead = async () => {
        try {
            await markAllAsRead().unwrap()
        } catch (error) {
            console.error("Error marking all notifications as read: ", error)
        }
    }

    if (!isAuthChecked) {
        return null
    }

    const unreadCount = notifications ? notifications?.filter((n: any) => !n.isRead).length : 0

    return (
        <header id="header" className="header fixed-top d-flex align-items-center">
            <div className="d-flex align-items-center justify-content-between" style={{ width: "230px" }}>
                <Link to="/home" className="logo d-flex align-items-center">
                    <img src={logo} alt="Logo" />
                </Link>
                <i className="bi bi-list toggle-sidebar-btn" onClick={toggleClass}></i>
            </div>

            {/* Notification */}
            <nav className="header-nav ms-auto">
                <ul className="d-flex align-items-center">
                    <li className="nav-item dropdown">
                        <a className="nav-link nav-icon" href="#" data-bs-toggle="dropdown" aria-expanded="false">
                            <i className="bi bi-bell"></i>
                            {unreadCount > 0 && <span className="badge bg-primary badge-number">{unreadCount}</span>}
                        </a>

                        <ul
                            className="dropdown-menu dropdown-menu-end dropdown-menu-arrow notifications p-0"
                            style={{ width: "35rem" }}
                        >
                            <li className="dropdown-header p-3 d-flex justify-content-between align-items-center">
                                <span>Bạn có {unreadCount} thông báo mới</span>
                                {unreadCount > 0 && (
                                    <button className="btn btn-link text-primary p-0 m-0" onClick={handleMarkAllAsRead}>
                                        Đánh dấu tất cả là đã đọc
                                    </button>
                                )}
                            </li>
                            <li>
                                <hr className="dropdown-divider m-0" />
                            </li>

                            <SimpleBar style={{ maxHeight: 400 }}>
                                <div className="list-group">
                                    {notifications && notifications.length > 0 ? (
                                        notifications?.map((notification: any) => (
                                            <a
                                                key={notification.id}
                                                href="#"
                                                className={`list-group-item list-group-item-action d-flex align-items-start ${
                                                    notification.isRead ? "list-group-item-light" : ""
                                                }`}
                                                onClick={() =>
                                                    handleNotificationClick(notification.id, notification.link)
                                                }
                                            >
                                                <i className="bi bi-info-circle text-primary me-3 mt-1"></i>
                                                <div className="w-100">
                                                    <p
                                                        className={`mb-1 ${
                                                            notification.isRead ? "fw-normal" : "fw-bold"
                                                        }`}
                                                    >
                                                        {notification.message}
                                                    </p>
                                                    <small className="text-muted">
                                                        {formatDistanceToNow(new Date(notification.createdAt), {
                                                            addSuffix: true,
                                                            locale: vi,
                                                        })}
                                                    </small>
                                                </div>
                                            </a>
                                        ))
                                    ) : (
                                        <div className="text-center p-3">
                                            <span>Không có thông báo nào.</span>
                                        </div>
                                    )}
                                </div>
                            </SimpleBar>
                        </ul>
                    </li>

                    <li className="nav-item dropdown pe-3">
                        <a
                            className="nav-link nav-profile d-flex align-items-center pe-0"
                            href="#"
                            data-bs-toggle="dropdown"
                            aria-expanded="false"
                        >
                            <span className="d-none d-md-block dropdown-toggle ps-2">{userData?.username}</span>
                        </a>

                        <ul className="dropdown-menu dropdown-menu-end dropdown-menu-arrow profile">
                            <li className="dropdown-header p-3">
                                <div className="text-start">
                                    <h6>{userData?.username}</h6>
                                    <span className="d-block">{getRoleNameVN(userData?.role)}</span>
                                </div>
                            </li>
                            <li>
                                <hr className="dropdown-divider m-0" />
                            </li>

                            <li>
                                <NavLink className="dropdown-item d-flex align-items-center p-3" to="/profile">
                                    <i className="bi bi-person me-2"></i>
                                    <span>Thông tin tài khoản</span>
                                </NavLink>
                            </li>
                            <li>
                                <hr className="dropdown-divider m-0" />
                            </li>

                            <li>
                                <NavLink className="dropdown-item d-flex align-items-center p-3" to="/change-password">
                                    <i className="bi bi-key me-2"></i>
                                    <span>Thay đổi mật khẩu</span>
                                </NavLink>
                            </li>
                            <li>
                                <hr className="dropdown-divider m-0" />
                            </li>

                            <li>
                                <button className="dropdown-item d-flex align-items-center p-3" onClick={handleLogout}>
                                    <i className="bi bi-box-arrow-right me-2"></i>
                                    <span>Đăng xuất</span>
                                </button>
                            </li>
                        </ul>
                    </li>
                </ul>
            </nav>
        </header>
    )
}

export default Header
