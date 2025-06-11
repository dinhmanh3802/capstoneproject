import { SD_Gender } from "../utility/SD"

export default interface FreeTimeModel {
    courseId: number
    date: string
    userId: number
    userName: string
    fullName: string
    gender: SD_Gender
    isCancel: boolean
}
