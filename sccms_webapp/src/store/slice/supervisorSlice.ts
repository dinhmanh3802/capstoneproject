// src/store/slice/supervisorSlice.ts
import { createSlice, PayloadAction } from "@reduxjs/toolkit"
import { supervisorModel } from "../../interfaces"

interface SupervisorState {
    supervisors: supervisorModel[]
    currentSupervisor: supervisorModel | null
    loading: boolean
    error: string | null
}

const initialState: SupervisorState = {
    supervisors: [],
    currentSupervisor: null,
    loading: false,
    error: null,
}

const supervisorSlice = createSlice({
    name: "supervisor",
    initialState,
    reducers: {
        getSupervisors: (state) => {
            state.loading = true
        },
        getSupervisorsSuccess: (state, action: PayloadAction<supervisorModel[]>) => {
            state.supervisors = action.payload
            state.loading = false
            state.error = null
        },
        getSupervisorsFailure: (state, action: PayloadAction<string>) => {
            state.loading = false
            state.error = action.payload
        },
        createSupervisor: (state) => {
            state.loading = true
        },
        createSupervisorSuccess: (state, action: PayloadAction<supervisorModel>) => {
            state.supervisors.push(action.payload)
            state.loading = false
            state.error = null
        },
        createSupervisorFailure: (state, action: PayloadAction<string>) => {
            state.loading = false
            state.error = action.payload
        },
        updateSupervisor: (state) => {
            state.loading = true
        },
        updateSupervisorSuccess: (state, action: PayloadAction<supervisorModel>) => {
            const index = state.supervisors?.findIndex((sup: any) => sup.id === action.payload.id)
            if (index !== -1) {
                state.supervisors[index] = action.payload
            }
            state.loading = false
            state.error = null
        },
        updateSupervisorFailure: (state, action: PayloadAction<string>) => {
            state.loading = false
            state.error = action.payload
        },
        deleteSupervisor: (state) => {
            state.loading = true
        },
        deleteSupervisorSuccess: (state, action: PayloadAction<number>) => {
            state.supervisors = state.supervisors?.filter((sup: any) => sup.id !== action.payload)
            state.loading = false
            state.error = null
        },
        deleteSupervisorFailure: (state, action: PayloadAction<string>) => {
            state.loading = false
            state.error = action.payload
        },
        setCurrentSupervisor: (state, action: PayloadAction<supervisorModel | null>) => {
            state.currentSupervisor = action.payload
        },
        deleteCurrentSupervisor: (state) => {
            state.currentSupervisor = null
        },
    },
})

export const {
    getSupervisors,
    getSupervisorsSuccess,
    getSupervisorsFailure,
    createSupervisor,
    createSupervisorSuccess,
    createSupervisorFailure,
    updateSupervisor,
    updateSupervisorSuccess,
    updateSupervisorFailure,
    deleteSupervisor,
    deleteSupervisorSuccess,
    deleteSupervisorFailure,
    setCurrentSupervisor,
    deleteCurrentSupervisor,
} = supervisorSlice.actions

export const supervisorReducer = supervisorSlice.reducer
