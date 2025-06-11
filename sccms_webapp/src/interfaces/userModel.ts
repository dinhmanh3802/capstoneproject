import { SD_Gender, SD_Role, SD_UserStatus } from "../utility/SD"

export interface userModel {
    id: number
    userName: string
    email: string
    fullName: string
    phoneNumber?: string
    gender?: SD_Gender
    dateOfBirth?: string
    address?: string
    nationalId: string
    status?: SD_UserStatus
    roleId: SD_Role
}
