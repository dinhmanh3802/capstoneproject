import { SD_Gender, SD_ProcessStatus } from "../utility/SD"
import studentGroupModel from "./studentGroupModel"

export default interface studentModel {
    id?: number
    fullName?: string
    dateOfBirth?: Date
    gender?: SD_Gender
    image?: string
    nationalId?: string
    nationalImageFront?: string
    nationalImageBack?: string
    address?: string
    parentName?: string
    emergencyContact?: string
    email?: string
    conduct?: string
    academicPerformance?: string
    status?: SD_ProcessStatus
    note?: string
    studentGroups: studentGroupModel
    studentCourses?: string[]
    studentGroupAssignment?: string[]
}
