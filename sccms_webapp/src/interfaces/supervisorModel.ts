import { SD_Gender, SD_UserStatus } from "../utility/SD"

export interface supervisorModel {
    id: number
    userName: string
    fullName: string
    email: string
    phoneNumber?: string
    gender?: SD_Gender
    address?: string
    nationalId: string
    dateOfBirth?: string
    status?: SD_UserStatus
    group?: {
        groupId: number
        groupName: string
    }
}
