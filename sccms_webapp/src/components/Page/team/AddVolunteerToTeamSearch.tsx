import React, { useState } from "react"
import { Form, Row, Col, Button, Accordion } from "react-bootstrap"
import Select from "react-select"
import { SD_Gender } from "../../../utility/SD"

const genderOptions = [
    { value: SD_Gender.Male, label: "Nam" },
    { value: SD_Gender.Female, label: "Nữ" },
]

// Giả định rằng teamList được truyền vào như một prop
interface AddVolunteerToTeamSearchProps {
    onSearch: (params: any) => void
    team: any
    teamList: Array<{ value: number; label: string }>
}

const AddVolunteerToTeamSearch: React.FC<AddVolunteerToTeamSearchProps> = ({ onSearch, team, teamList }) => {
    const [searchParams, setSearchParams] = useState({
        courseId: team.courseId,
        volunteerCode: "",
        name: "",
        phoneNumber: "",
        gender: "",
        teamId: "",
    })

    const handleChange = (field: string) => (e: React.ChangeEvent<HTMLInputElement>) => {
        setSearchParams({
            ...searchParams,
            [field]: e.target.value,
        })
    }

    const handleSelectChange = (field: string) => (option: any) => {
        setSearchParams({
            ...searchParams,
            [field]: option ? option.value : "",
        })
    }

    const handleSearch = (e: React.FormEvent) => {
        e.preventDefault()
        onSearch(searchParams)
    }

    const handleReset = () => {
        const resetParams = {
            courseId: team.courseId,
            volunteerCode: "",
            name: "",
            phoneNumber: "",
            gender: "",
            teamId: "",
        }
        setSearchParams(resetParams)
        onSearch(resetParams)
    }

    return (
        <Accordion>
            <Accordion.Item eventKey="0">
                <Accordion.Header>
                    <i className="bi bi-search me-2"></i> Tìm kiếm
                </Accordion.Header>
                <Accordion.Body>
                    <Form onSubmit={handleSearch} className="mb-3">
                        <Row>
                            <Col md={3}>
                                <Form.Group controlId="volunteerCode">
                                    <Form.Label>Mã TNV</Form.Label>
                                    <Form.Control
                                        type="text"
                                        placeholder="Mã TNV"
                                        value={searchParams.volunteerCode}
                                        onChange={handleChange("volunteerCode")}
                                    />
                                </Form.Group>
                            </Col>
                            <Col md={3}>
                                <Form.Group controlId="name">
                                    <Form.Label>Tên</Form.Label>
                                    <Form.Control
                                        type="text"
                                        placeholder="Tên"
                                        value={searchParams.name}
                                        onChange={handleChange("name")}
                                    />
                                </Form.Group>
                            </Col>
                            <Col md={3}>
                                <Form.Group controlId="phoneNumber">
                                    <Form.Label>Số điện thoại</Form.Label>
                                    <Form.Control
                                        type="text"
                                        placeholder="Số điện thoại"
                                        value={searchParams.phoneNumber}
                                        onChange={handleChange("phoneNumber")}
                                    />
                                </Form.Group>
                            </Col>
                            <Col md={3}>
                                <Form.Group controlId="gender">
                                    <Form.Label>Giới tính</Form.Label>
                                    <Select
                                        options={genderOptions}
                                        onChange={handleSelectChange("gender")} // @ts-ignore
                                        value={genderOptions?.find((option) => option.value === searchParams.gender)}
                                        isClearable
                                    />
                                </Form.Group>
                            </Col>
                            <Col md={3}>
                                <Form.Group controlId="team">
                                    <Form.Label>Ban</Form.Label>
                                    <Select
                                        options={teamList?.filter((option) => option.value !== team.id)}
                                        onChange={handleSelectChange("teamId")} // @ts-ignore
                                        value={teamList?.find((option) => option.value === searchParams.teamId)}
                                        isClearable
                                    />
                                </Form.Group>
                            </Col>
                            <Col md={9}>
                                <div className="text-end mt-4">
                                    <Button type="submit" variant="primary">
                                        Tìm kiếm
                                    </Button>
                                    <Button type="button" variant="secondary" className="ms-2" onClick={handleReset}>
                                        Xóa
                                    </Button>
                                </div>
                            </Col>
                        </Row>
                    </Form>
                </Accordion.Body>
            </Accordion.Item>
        </Accordion>
    )
}

export default AddVolunteerToTeamSearch
