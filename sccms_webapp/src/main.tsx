import { createRoot } from "react-dom/client"
import { Provider } from "react-redux"
import { RouterProvider } from "react-router-dom"
import { router } from "./router/router"
import { store } from "./store"
import { ToastContainer } from "react-toastify"
import "bootstrap/dist/css/bootstrap.css"
import "bootstrap/dist/js/bootstrap.js"
import "react-toastify/dist/ReactToastify.css"
import { ErrorBoundary } from "./pages"

createRoot(document.getElementById("root")!).render(
    <Provider store={store}>
        <ErrorBoundary>
            <RouterProvider router={router} />
            <ToastContainer />
        </ErrorBoundary>
    </Provider>,
)
