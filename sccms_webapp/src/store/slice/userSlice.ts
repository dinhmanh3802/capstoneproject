import { createSlice, PayloadAction } from "@reduxjs/toolkit"
import { userModel } from "../../interfaces/userModel"

interface UserState {
    users: userModel[]
    currentUser: userModel | null // Thêm thuộc tính currentUser
    loading: boolean
    error: string | null
}

const initialState: UserState = {
    users: [],
    currentUser: null, // Khởi tạo currentUser là null
    loading: false,
    error: null,
}

const userSlice = createSlice({
    name: "user",
    initialState,
    reducers: {
        getUsers: (state) => {
            state.loading = true
        },
        getUsersSuccess: (state, action: PayloadAction<userModel[]>) => {
            state.users = action.payload
            state.loading = false
            state.error = null
        },
        getUsersFailure: (state, action: PayloadAction<string>) => {
            state.loading = false
            state.error = action.payload
        },
        createUser: (state) => {
            state.loading = true
        },
        createUserSuccess: (state, action: PayloadAction<userModel>) => {
            state.users.push(action.payload)
            state.loading = false
            state.error = null
        },
        createUserFailure: (state, action: PayloadAction<string>) => {
            state.loading = false
            state.error = action.payload
        },
        updateUser: (state) => {
            state.loading = true
        },
        updateUserSuccess: (state, action: PayloadAction<userModel>) => {
            const index = state.users?.findIndex((user) => user.userName === action.payload.userName)
            if (index !== -1) {
                state.users[index] = action.payload
            }
            state.loading = false
            state.error = null
        },
        updateUserFailure: (state, action: PayloadAction<string>) => {
            state.loading = false
            state.error = action.payload
        },
        deleteUser: (state) => {
            state.loading = true
        },
        deleteUserSuccess: (state, action: PayloadAction<{ userName: string }>) => {
            state.users = state.users?.filter((user) => user.userName !== action.payload.userName)
            state.loading = false
            state.error = null
        },
        deleteUserFailure: (state, action: PayloadAction<string>) => {
            state.loading = false
            state.error = action.payload
        },
        changePassword: (state) => {
            state.loading = true
        },
        changePasswordSuccess: (state) => {
            state.loading = false
            state.error = null
        },
        changePasswordFailure: (state, action: PayloadAction<string>) => {
            state.loading = false
            state.error = action.payload
        },
        changeUserStatus: (state) => {
            state.loading = true
        },
        changeUserStatusSuccess: (state, action: PayloadAction<userModel>) => {
            const index = state.users?.findIndex((user) => user.id === action.payload.id)
            if (index !== -1) {
                state.users[index] = action.payload
            }
            state.loading = false
            state.error = null
        },
        changeUserStatusFailure: (state, action: PayloadAction<string>) => {
            state.loading = false
            state.error = action.payload
        },
        setCurrentUser: (state, action: PayloadAction<userModel | null>) => {
            state.currentUser = action.payload
        },
        deleteCurrentUser: (state) => {
            state.currentUser = null
        },
    },
})

export const {
    getUsers,
    getUsersSuccess,
    getUsersFailure,
    createUser,
    createUserSuccess,
    createUserFailure,
    updateUser,
    updateUserSuccess,
    updateUserFailure,
    deleteUser,
    deleteUserSuccess,
    deleteUserFailure,
    changePassword,
    changePasswordSuccess,
    changePasswordFailure,
    changeUserStatus,
    changeUserStatusSuccess,
    changeUserStatusFailure,
    setCurrentUser, // Xuất hành động để đặt người dùng hiện tại
    deleteCurrentUser, // Xuất hành động để xóa người dùng hiện tại
} = userSlice.actions

export const userReducer = userSlice.reducer
