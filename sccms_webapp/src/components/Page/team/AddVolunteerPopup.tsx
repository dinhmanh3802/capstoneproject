import React, { useEffect, useState } from "react"
import { Modal, Button } from "react-bootstrap"
import AddVolunteerToTeamList from "./AddVolunteerToTeamList"
import AddVolunteerToTeamSearch from "./AddVolunteerToTeamSearch"
import { useGetTeamsByCourseIdQuery } from "../../../apis/teamApi" // Import hook để lấy teamList
import { te } from "date-fns/locale"
import { toastNotify } from "../../../helper"

interface AddVolunteerPopupProps {
    show: boolean
    onHide: () => void
    onAddVolunteers: any
    team: { courseId: number }
}

const AddVolunteerPopup: React.FC<AddVolunteerPopupProps> = ({ show, onHide, onAddVolunteers, team }) => {
    const [selectedVolunteers, setSelectedVolunteers] = useState<number[]>([])
    const [searchParams, setSearchParams] = useState({
        courseId: team.courseId,
        volunteerCode: "",
        name: "",
        phoneNumber: "",
        gender: "",
        status: "",
        teamId: "",
    })
    useEffect(() => {
        setSearchParams({
            courseId: team.courseId,
            volunteerCode: "",
            name: "",
            phoneNumber: "",
            gender: "",
            status: "",
            teamId: "",
        })
    }, [show])

    // Gọi API lấy danh sách team dựa trên courseId
    const { data: teamData, isLoading: teamLoading } = useGetTeamsByCourseIdQuery(team.courseId)

    // Chuyển đổi teamData thành teamList cho Select component
    const teamList =
        teamData?.result?.map((team: any) => ({
            value: team.id,
            label: team.teamName,
        })) || []
    teamList.unshift({ value: 0, label: "Chưa phân" })
    const handleSearch = (params: any) => {
        setSearchParams(params)
    }

    const handleSelectVolunteers = (volunteerIds: number[]) => {
        setSelectedVolunteers(volunteerIds)
    }

    const handleAdd = async () => {
        const IsAddSuccess = onAddVolunteers(selectedVolunteers)
        if (await IsAddSuccess) setSelectedVolunteers([])
    }

    return (
        <Modal show={show} onHide={onHide} size="lg">
            <Modal.Header closeButton>
                <Modal.Title>Thêm tình nguyện viên vào ban</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                {/* Kiểm tra loading của teamList */}
                {teamLoading ? (
                    <p>Loading...</p>
                ) : (
                    <>
                        <AddVolunteerToTeamSearch onSearch={handleSearch} team={team} teamList={teamList} />
                        <AddVolunteerToTeamList
                            searchParams={searchParams}
                            onSelectVolunteers={handleSelectVolunteers} // @ts-ignore
                            teamId={team.id}
                            teamList={teamList}
                        />
                    </>
                )}
            </Modal.Body>
            <Modal.Footer>
                <Button variant="secondary" onClick={onHide}>
                    Hủy
                </Button>
                <Button
                    variant="primary"
                    onClick={() => {
                        if (selectedVolunteers.length > 0) {
                            handleAdd()
                        } else {
                            toastNotify("Vui lòng chọn tình nguyện viên để thêm", "error")
                        }
                    }}
                >
                    Thêm
                </Button>
            </Modal.Footer>
        </Modal>
    )
}

export default AddVolunteerPopup
