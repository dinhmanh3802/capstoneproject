import { nightShiftModel, roomModel, userModel } from "."
import { SD_NightShiftAssignmentStatus } from "../utility/SD"

export default interface nightShiftAssignmentModel {
    id: number
    nightShiftId: number
    nightShift?: nightShiftModel
    userId?: number
    user?: userModel
    date: string // Định dạng "YYYY-MM-DD"
    roomId?: number
    room: roomModel
    status: SD_NightShiftAssignmentStatus
    rejectionReason?: string
}
