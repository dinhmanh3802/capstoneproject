import React, { useState, useEffect } from "react"
import { useParams } from "react-router-dom"
import { useSelector } from "react-redux"
import { Form, Row, Col, Button, Alert } from "react-bootstrap"
import Select from "react-select"
import { useGetTeamByIdQuery, useUpdateTeamMutation } from "../../apis/teamApi"
import { useGetUsersQuery } from "../../apis/userApi"
import { SD_Gender } from "../../utility/SD"
import { RootState } from "../../store/store"
import TeamVolunteers from "../../components/Page/team/TeamVolunteers"
import { toast } from "react-toastify"
import { MainLoader } from "../../components/Page"

const TeamDetail: React.FC = () => {
    const { teamId } = useParams<{ teamId: string }>()
    const { data: teamDataResponse, isLoading: teamLoading } = useGetTeamByIdQuery(Number(teamId))
    const { data: userData } = useGetUsersQuery({})
    const [updateTeam] = useUpdateTeamMutation()

    const currentUserRole = useSelector((state: RootState) => state.auth.user?.role)
    const currentCourse = useSelector((state: RootState) => state.courseStore.currentCourse)

    const [teamName, setTeamName] = useState("")
    const [gender, setGender] = useState<SD_Gender | null>(null)
    const [expectedVolunteers, setExpectedVolunteers] = useState<number | undefined>(undefined)
    const [courseId, setCourseId] = useState<number | undefined>(undefined)
    const [leader, setLeader] = useState<{ value: number; label: string } | null>(null)
    const [isEditing, setIsEditing] = useState(false)
    const [showSuccess, setShowSuccess] = useState(false)
    const [errors, setErrors] = useState<{ [key: string]: string }>({})

    const loadInitialData = () => {
        if (teamDataResponse?.isSuccess && teamDataResponse.result) {
            const { teamName, gender, expectedVolunteers, leaderId, courseId } = teamDataResponse.result
            setTeamName(teamName || "")
            setGender(
                gender === SD_Gender.Male ? SD_Gender.Male : gender === SD_Gender.Female ? SD_Gender.Female : null,
            )
            setExpectedVolunteers(expectedVolunteers ? Number(expectedVolunteers) : undefined)
            setCourseId(courseId ? Number(courseId) : undefined)
            const leaderUser = userData?.result?.find((user: any) => user.id === Number(leaderId))
            setLeader(
                leaderUser ? { value: leaderUser.id, label: `${leaderUser.userName}: ${leaderUser.fullName}` } : null,
            )
        }
    }

    useEffect(() => {
        loadInitialData()
    }, [teamDataResponse, userData])

    if (teamLoading) return <MainLoader />

    const filteredLeaders =
        userData?.result
            ?.filter((user: any) => user.roleId === 3 && (gender === null || user.gender === gender))
            ?.map((user: any) => ({
                value: user.id,
                label: `${user.userName}: ${user.fullName}`,
            })) ?? []

    const validateForm = () => {
        const newErrors: { [key: string]: string } = {}
        if (!teamName) newErrors.teamName = "Tên ban không được để trống"
        if (!expectedVolunteers || expectedVolunteers < 0 || expectedVolunteers > 1000) {
            newErrors.expectedVolunteers = "Số lượng dự kiến phải từ 0 đến 1000"
        }
        if (!leader) newErrors.leader = "Trưởng ban không được để trống"
        setErrors(newErrors)
        return Object.keys(newErrors).length === 0
    }

    const handleSave = async () => {
        if (!validateForm()) return

        try {
            const result = await updateTeam({
                id: Number(teamId),
                teamName,
                gender,
                courseId,
                expectedVolunteers,
                leaderId: leader?.value,
            }).unwrap()
            setIsEditing(false)
            setShowSuccess(true)
            setTimeout(() => setShowSuccess(false), 3000)
        } catch (error: any) {
            toast.error(error?.data.errorMessages.join(", "))
        }
    }

    const handleCancel = () => {
        loadInitialData()
        setIsEditing(false)
        setErrors({})
    }

    const handleGenderChange = (option: any) => {
        setGender(option ? option.value : null)
        setLeader(null)
    }

    const selectStyles = {
        control: (base: any) => ({
            ...base,
            height: "40px",
            minHeight: "40px",
        }),
    }

    const inputStyle = { height: "40px" }

    return (
        <div className="container">
            <h3 className="fw-bold primary-color">Chi tiết ban - {teamName}</h3>
            <Form>
                <Row>
                    <Form.Group as={Col} controlId="teamName" className="mb-3">
                        <Form.Label>Tên ban {isEditing && <span className="text-danger">*</span>}</Form.Label>
                        <Form.Control
                            type="text"
                            placeholder="Nhập tên ban"
                            value={teamName}
                            onChange={(e) => setTeamName(e.target.value)}
                            disabled={!isEditing}
                            isInvalid={!!errors.teamName}
                            style={inputStyle}
                        />
                        <Form.Control.Feedback type="invalid">{errors.teamName}</Form.Control.Feedback>
                    </Form.Group>
                    <Form.Group as={Col} controlId="gender" className="mb-3">
                        <Form.Label>Giới tính {isEditing && <span className="text-danger">*</span>}</Form.Label>
                        <Select
                            options={[
                                { value: SD_Gender.Male, label: "Nam" },
                                { value: SD_Gender.Female, label: "Nữ" },
                                { value: null, label: "Tất cả" },
                            ]}
                            onChange={handleGenderChange}
                            value={
                                gender === null
                                    ? { value: null, label: "Tất cả" }
                                    : gender === SD_Gender.Male
                                    ? { value: SD_Gender.Male, label: "Nam" }
                                    : { value: SD_Gender.Female, label: "Nữ" }
                            }
                            isDisabled={true}
                            styles={selectStyles}
                        />
                    </Form.Group>
                    <Form.Group as={Col} controlId="expectedVolunteers" className="mb-3">
                        <Form.Label>Số lượng dự kiến {isEditing && <span className="text-danger">*</span>}</Form.Label>
                        <Form.Control
                            type="number"
                            placeholder="Nhập số lượng dự kiến"
                            value={expectedVolunteers ?? ""}
                            onChange={(e) => setExpectedVolunteers(Number(e.target.value))}
                            disabled={!isEditing}
                            isInvalid={!!errors.expectedVolunteers}
                            style={inputStyle}
                        />
                        <Form.Control.Feedback type="invalid">{errors.expectedVolunteers}</Form.Control.Feedback>
                    </Form.Group>
                    <Form.Group as={Col} controlId="leader" className="mb-3">
                        <Form.Label>Trưởng ban {isEditing && <span className="text-danger">*</span>}</Form.Label>
                        <Select
                            options={filteredLeaders}
                            onChange={(option) => setLeader(option)}
                            value={leader}
                            placeholder="Chọn trưởng ban"
                            isDisabled={!isEditing}
                            styles={selectStyles}
                            className={errors.leader ? "is-invalid" : ""}
                        />
                        {errors.leader && <div className="invalid-feedback d-block">{errors.leader}</div>}
                    </Form.Group>
                </Row>
            </Form>
            <TeamVolunteers teamId={teamId} team={teamDataResponse.result} />
        </div>
    )
}

export default TeamDetail
