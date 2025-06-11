import { SD_PostType } from "../utility/SD"

const getPostType = (status: SD_PostType): string => {
    switch (status) {
        case SD_PostType.Introduction:
            return "Giới thiệu"
        case SD_PostType.Activities:
            return "Hoạt động"
        case SD_PostType.Announcement:
            return "Thông báo"
    }
}

export { getPostType }
