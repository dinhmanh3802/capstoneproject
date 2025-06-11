import React, { useEffect, useState } from "react"
import { Accordion, Form, Button, Row, Col } from "react-bootstrap"
import Select from "react-select"
import { useNavigate } from "react-router-dom"
import { SD_Gender, SD_ProcessStatus } from "../../../utility/SD"
import { button } from "../../../utility/Label"

const statusOptions = [
    { value: SD_ProcessStatus.Approved, label: "Chờ nhập học" },
    { value: SD_ProcessStatus.Enrolled, label: "Nhập học" },
    { value: SD_ProcessStatus.Graduated, label: "Tốt nghiệp" },
    { value: SD_ProcessStatus.DropOut, label: "Bỏ học" },
]

const genderOptions = [
    { value: SD_Gender.Male, label: "Nam" },
    { value: SD_Gender.Female, label: "Nữ" },
]

const VolunteerSearchByTeam = ({ onSearch }) => {
    const navigate = useNavigate()
    const [searchParams, setSearchParams] = useState({
        volunteerCode: "",
        fullName: "",
        phoneNumber: "",
        status: "",
        gender: "",
    })

    useEffect(() => {
        const searchParams = new URLSearchParams(location.search)
        const initialData: any = {
            volunteerCode: searchParams.get("volunteerCode") || "",
            name: searchParams.get("fullName") || "",
            phoneNumber: searchParams.get("phoneNumber") || "",
            status: searchParams.get("status") ? parseInt(searchParams.get("status")!) : "",
            gender: searchParams.get("gender") ? parseInt(searchParams.get("gender")!) : "",
        }
        setSearchParams(initialData)
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [location.search])

    const handleInputChange = (e) => {
        setSearchParams({
            ...searchParams,
            [e.target.name]: e.target.value,
        })
    }

    const handleSelectChange = (field) => (option) => {
        setSearchParams({
            ...searchParams,
            [field]: option ? option.value : "",
        })
    }

    const handleSearch = (e) => {
        e.preventDefault()
        const queryParams = new URLSearchParams(searchParams as any).toString()
        navigate(`?${queryParams}`) // Cập nhật URL với các tham số tìm kiếm
        onSearch(searchParams) // Gọi hàm onSearch để xử lý tìm kiếm
    }

    const handleReset = () => {
        const resetParams = {
            volunteerCode: "",
            fullName: "",
            phoneNumber: "",
            status: "",
            gender: "",
        }
        setSearchParams(resetParams)
        navigate("") // Xóa tham số tìm kiếm khỏi URL
        onSearch(resetParams)
    }

    return (
        <Accordion>
            <Accordion.Item eventKey="0">
                <Accordion.Header>
                    <i className="bi bi-search me-2"></i>
                    {button.search}
                </Accordion.Header>{" "}
                <Accordion.Body>
                    <Form onSubmit={handleSearch}>
                        <Row className="mb-3">
                            <Col md={3}>
                                <Form.Group>
                                    <Form.Label>Mã TNV</Form.Label>
                                    <Form.Control
                                        type="text"
                                        name="volunteerCode"
                                        value={searchParams.volunteerCode}
                                        onChange={handleInputChange}
                                    />
                                </Form.Group>
                            </Col>
                            <Col md={3}>
                                <Form.Group>
                                    <Form.Label>Tên</Form.Label>
                                    <Form.Control
                                        type="text"
                                        name="fullName"
                                        value={searchParams.fullName}
                                        onChange={handleInputChange}
                                    />
                                </Form.Group>
                            </Col>
                            <Col md={3}>
                                <Form.Group>
                                    <Form.Label>Số điện thoại</Form.Label>
                                    <Form.Control
                                        type="text"
                                        name="phoneNumber"
                                        value={searchParams.phoneNumber}
                                        onChange={handleInputChange}
                                    />
                                </Form.Group>
                            </Col>
                            <Col md={3}>
                                <Form.Group>
                                    <Form.Label>Giới tính</Form.Label>
                                    <Select
                                        options={genderOptions}
                                        onChange={handleSelectChange("gender")}
                                        isClearable // @ts-ignore
                                        value={genderOptions?.find((opt) => opt.value === searchParams.gender) || null}
                                        placeholder="Chọn giới tính"
                                    />
                                </Form.Group>
                            </Col>
                        </Row>
                        <Row className="mb-3">
                            <Col md={3}>
                                <Form.Group>
                                    <Form.Label>Trạng thái</Form.Label>
                                    <Select
                                        options={statusOptions}
                                        onChange={handleSelectChange("status")} // @ts-ignore
                                        value={statusOptions?.find((opt) => opt.value === searchParams.status) || null}
                                        isClearable
                                        placeholder="Chọn trạng thái"
                                    />
                                </Form.Group>
                            </Col>
                            <Col md={9} className="d-flex flex-column align-items-end mt-auto">
                                <div>
                                    <Button variant="primary" type="submit" className="me-2">
                                        Tìm kiếm
                                    </Button>
                                    <Button variant="secondary" onClick={handleReset}>
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

export default VolunteerSearchByTeam
