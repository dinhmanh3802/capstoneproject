import { useSelector } from "react-redux"
import { Link, useLocation, useParams } from "react-router-dom"
import { RootState } from "../../store/store"
import { SD_Role_Name } from "../../utility/SD"

function Navbar() {
    const location = useLocation() // Lấy thông tin URL hiện tại
    const courseId = useParams().courseId
    const currentUserRole = useSelector((state: RootState) => state.auth.user?.role)
    const currentCourse = useSelector((state: RootState) => state.courseStore.currentCourse)
    if (currentUserRole == SD_Role_Name.ADMIN) {
        return (
            <aside id="sidebar" className="sidebar">
                <ul className="sidebar-nav" id="sidebar-nav">
                    <li className="nav-item">
                        <Link
                            className={`nav-link ${location.pathname === "/user-list" ? "" : "collapsed"}`}
                            to="/user-list"
                        >
                            <i className="bi bi-person-gear"></i>
                            <span>Người dùng</span>
                        </Link>
                    </li>
                </ul>
            </aside>
        )
    }
    return (
        <aside id="sidebar" className="sidebar">
            <ul className="sidebar-nav" id="sidebar-nav">
                <li className="nav-heading">Danh Mục</li>
                <li className="nav-item">
                    <Link className={`nav-link ${location.pathname === "/" ? "" : "collapsed"}`} to={`/`}>
                        <i className="bi bi-bar-chart"></i>
                        <span>Dashboard</span>
                    </Link>
                </li>
                <li className="nav-item">
                    <Link
                        className={`nav-link ${location.pathname.startsWith("/course") ? "" : "collapsed"}`}
                        to="/course"
                    >
                        <i className="bi bi-bank"></i>
                        <span>Khóa tu</span>
                    </Link>
                </li>
                <li className="nav-item">
                    <Link
                        className={`nav-link ${location.pathname.startsWith("/team") ? "" : "collapsed"}`}
                        to={`/team`}
                    >
                        <i className="bi bi-backpack4"></i>
                        <span>Ban</span>
                    </Link>
                </li>
                <li className="nav-item">
                    <Link
                        className={`nav-link ${location.pathname.startsWith("/student-groups") ? "" : "collapsed"}`}
                        to={`/student-groups`}
                    >
                        <i className="bi bi-people-fill"></i>
                        <span>Chánh</span>
                    </Link>
                </li>
                <li className="nav-item">
                    <Link
                        className={`nav-link ${location.pathname.startsWith("/students") ? "" : "collapsed"}`}
                        to={`/students`}
                    >
                        <i className="bi bi-person-badge"></i>
                        <span>Khóa sinh</span>
                    </Link>
                </li>
                <li className="nav-item">
                    <Link
                        className={`nav-link ${location.pathname.startsWith("/volunteers") ? "" : "collapsed"}`}
                        to={`/volunteers`}
                    >
                        <i className="bi bi-star"></i>
                        <span>Tình nguyện viên</span>
                    </Link>
                </li>
                {currentUserRole === SD_Role_Name.MANAGER && (
                    <li className="nav-item">
                        <Link
                            className={`nav-link ${
                                location.pathname.startsWith("/supervisor-list") ? "" : "collapsed"
                            }`}
                            to={`/supervisor-list`}
                        >
                            <i className="bi bi-shield-minus"></i>
                            <span>Huynh trưởng</span>
                        </Link>
                    </li>
                )}

                <li className="nav-item">
                    <Link
                        className={`nav-link ${location.pathname.startsWith("/attendance-reports") ? "" : "collapsed"}`}
                        to={`/attendance-reports`}
                    >
                        <i className="bi bi-clipboard-check"></i>
                        <span>Báo cáo hằng ngày</span>
                    </Link>
                </li>
                <li className="nav-item">
                    <a className="nav-link collapsed" data-bs-target="#trucDem-nav" data-bs-toggle="collapse" href="#">
                        <i className="bi bi-moon"></i>
                        <span>Trực đêm</span>
                        <i className="bi bi-chevron-down ms-auto"></i>
                    </a>
                    <ul id="trucDem-nav" className="nav-content collapse " data-bs-parent="#sidebar-nav">
                        <li>
                            <Link to={"my-night-shift"}>
                                <i className="bi bi-circle"></i>
                                <span>Lịch trực</span>
                            </Link>
                        </li>
                        <li>
                            <Link to={"night-shift-manager"}>
                                <i className="bi bi-circle"></i>
                                <span>Danh sách ca trực</span>
                            </Link>
                        </li>
                        {currentUserRole === SD_Role_Name.STAFF && (
                            <li>
                                <Link to={"free-time"}>
                                    <i className="bi bi-circle"></i>
                                    <span>Đăng ký ngày trực</span>
                                </Link>
                            </li>
                        )}

                        {(currentUserRole === SD_Role_Name.MANAGER || currentUserRole === SD_Role_Name.SECRETARY) && (
                            <div>
                                <li>
                                    <Link to={"free-time-manager"}>
                                        <i className="bi bi-circle"></i>
                                        <span>Danh sách đăng ký</span>
                                    </Link>
                                </li>
                                <li>
                                    <Link to={"reject-night-shift"}>
                                        <i className="bi bi-circle"></i>
                                        <span>Danh sách hủy ca</span>
                                    </Link>
                                </li>
                            </div>
                        )}
                        {currentUserRole === SD_Role_Name.MANAGER && (
                            <div>
                                <li>
                                    <Link to={"night-shift-config"}>
                                        <i className="bi bi-circle"></i>
                                        <span>Cấu hình</span>
                                    </Link>
                                </li>
                            </div>
                        )}
                    </ul>
                </li>
                {(currentUserRole === SD_Role_Name.MANAGER || currentUserRole === SD_Role_Name.SECRETARY) && (
                    <li className="nav-item">
                        <a
                            className="nav-link collapsed"
                            data-bs-target="#khoa_sinh_nav"
                            data-bs-toggle="collapse"
                            href="#"
                        >
                            <i className="bi bi-file-earmark-text"></i>
                            <span>Đơn đăng ký</span>
                            <i className="bi bi-chevron-down ms-auto"></i>
                        </a>
                        <ul id="khoa_sinh_nav" className="nav-content collapse " data-bs-parent="#sidebar-nav">
                            <li>
                                <Link to="/student-applications">
                                    <i className="bi bi-circle"></i>
                                    <span>Đăng ký khóa sinh</span>
                                </Link>
                            </li>
                            <li>
                                <Link to="/volunteer-applications">
                                    <i className="bi bi-circle"></i>
                                    <span>Đăng ký tình nguyện viên</span>
                                </Link>
                            </li>
                        </ul>
                    </li>
                )}

                {currentUserRole === SD_Role_Name.MANAGER && (
                    <>
                        <li className="nav-heading mt-4"></li>
                        <li className="nav-heading">Quản lý</li>
                        <li className="nav-item">
                            <Link
                                className={`nav-link ${location.pathname.startsWith("/post") ? "" : "collapsed"}`}
                                to={`/post`}
                            >
                                <i className="bi bi-newspaper"></i>
                                <span> Bài đăng</span>
                            </Link>
                        </li>
                        <li className="nav-item">
                            <Link
                                className={`nav-link ${location.pathname.startsWith("/feedback") ? "" : "collapsed"}`}
                                to={`/feedback`}
                            >
                                <i className="bi bi-chat-dots"></i>
                                <span> Phản hồi</span>
                            </Link>
                        </li>
                        <li className="nav-item">
                            <Link
                                className={`nav-link ${location.pathname === "/user-list" ? "" : "collapsed"}`}
                                to="/user-list"
                            >
                                <i className="bi bi-person-gear"></i>
                                <span>Người dùng</span>
                            </Link>
                        </li>
                    </>
                )}
            </ul>
        </aside>
    )
}

export default Navbar
