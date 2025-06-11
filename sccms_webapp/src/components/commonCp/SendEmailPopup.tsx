import React, { useState } from "react"
import { Modal, Button, Form } from "react-bootstrap"
import { CKEditor } from "@ckeditor/ckeditor5-react"
import {
    AccessibilityHelp,
    Autoformat,
    AutoImage,
    Autosave,
    Base64UploadAdapter,
    BlockQuote,
    Bold,
    ClassicEditor,
    Essentials,
    Heading,
    ImageBlock,
    ImageCaption,
    ImageInline,
    ImageInsert,
    ImageInsertViaUrl,
    ImageResize,
    ImageStyle,
    ImageTextAlternative,
    ImageToolbar,
    ImageUpload,
    Indent,
    IndentBlock,
    Italic,
    Link,
    LinkImage,
    Paragraph,
    SelectAll,
    Table,
    TableCaption,
    TableCellProperties,
    TableColumnResize,
    TableProperties,
    TableToolbar,
    TextTransformation,
    Underline,
    Undo,
} from "ckeditor5"
import "ckeditor5/ckeditor5.css"
import emailTemplateModel from "../../interfaces/emailTemplateModel"

interface SendResultPopupProps {
    isOpen: boolean
    onClose: () => void
    onConfirm: (title: string, content: string) => void
    listTemplate: emailTemplateModel[]
    select?: number // Template mặc định
    onClearSelectedRows: () => void
}

const SendEmailPopup: React.FC<SendResultPopupProps> = ({
    isOpen,
    onClose,
    onConfirm,
    listTemplate,
    select = 1,
    onClearSelectedRows,
}) => {
    const defaultTemplate = listTemplate?.find((template) => template.id === select) || listTemplate[0]
    const [title, setTitle] = useState(defaultTemplate.subject)
    const [content, setContent] = useState(defaultTemplate.body)
    const [selectedTemplate, setSelectedTemplate] = useState(defaultTemplate.name)

    const handleTemplateChange = (event: any) => {
        const selected = event.target.value
        setSelectedTemplate(selected)
        const template = listTemplate?.find((template) => template.name === selected)
        if (template) {
            setTitle(template.subject)
            setContent(template.body)
        }
    }

    const handleSendResultConfirm = () => {
        onConfirm(title, content)
        onClearSelectedRows()
    }
    const editorConfig = {
        toolbar: {
            items: [
                "undo",
                "redo",
                "|",
                "heading",
                "|",
                "bold",
                "italic",
                "underline",
                "|",
                "link",
                "insertImage",
                "insertTable",
                "blockQuote",
                "|",
                "outdent",
                "indent",
            ],
            shouldNotGroupWhenFull: false,
        },
        plugins: [
            AccessibilityHelp,
            Autoformat,
            AutoImage,
            Autosave,
            Base64UploadAdapter,
            BlockQuote,
            Bold,
            Essentials,
            Heading,
            ImageBlock,
            ImageCaption,
            ImageInline,
            ImageInsert,
            ImageInsertViaUrl,
            ImageResize,
            ImageStyle,
            ImageTextAlternative,
            ImageToolbar,
            ImageUpload,
            Indent,
            IndentBlock,
            Italic,
            Link,
            LinkImage,
            Paragraph,
            SelectAll,
            Table,
            TableCaption,
            TableCellProperties,
            TableColumnResize,
            TableProperties,
            TableToolbar,
            TextTransformation,
            Underline,
            Undo,
        ],
        heading: {
            options: [
                { model: "paragraph", title: "Paragraph", class: "ck-heading_paragraph" },
                { model: "heading1", view: "h1", title: "Heading 1", class: "ck-heading_heading1" },
                { model: "heading2", view: "h2", title: "Heading 2", class: "ck-heading_heading2" },
                { model: "heading3", view: "h3", title: "Heading 3", class: "ck-heading_heading3" },
            ],
        },
        image: {
            toolbar: [
                "toggleImageCaption",
                "imageTextAlternative",
                "|",
                "imageStyle:inline",
                "imageStyle:wrapText",
                "imageStyle:breakText",
                "|",
                "resizeImage",
            ],
        },
        placeholder: "Type or paste your content here!",
        table: {
            contentToolbar: ["tableColumn", "tableRow", "mergeTableCells", "tableProperties", "tableCellProperties"],
        },
    }

    return (
        <Modal show={isOpen} onHide={onClose} size="lg">
            <Modal.Header closeButton>
                <Modal.Title>Gửi Kết Quả</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                <Form>
                    <Form.Group controlId="formTemplate">
                        <Form.Label>Chọn Template</Form.Label>
                        <Form.Control as="select" value={selectedTemplate} onChange={handleTemplateChange}>
                            {listTemplate?.map((template) => (
                                <option key={template.name} value={template.name}>
                                    {template.name}
                                </option>
                            ))}
                        </Form.Control>
                    </Form.Group>
                    <Form.Group controlId="formTitle" className="mt-3">
                        <Form.Label>Tiêu đề</Form.Label>
                        <Form.Control
                            type="text"
                            value={title}
                            onChange={(e) => setTitle(e.target.value)}
                            placeholder="Nhập tiêu đề"
                        />
                    </Form.Group>
                    <Form.Group controlId="formContent" className="mt-3">
                        <Form.Label>Nội dung</Form.Label>
                        <div style={{ minHeight: "20rem" }}>
                            <CKEditor
                                editor={ClassicEditor} // @ts-ignore
                                config={editorConfig}
                                data={content}
                                onChange={(event, editor) => {
                                    const data = editor.getData()
                                    setContent(data)
                                }}
                            />
                        </div>
                    </Form.Group>
                </Form>
            </Modal.Body>
            <Modal.Footer>
                <Button variant="secondary" onClick={onClose}>
                    Hủy
                </Button>
                <Button variant="primary" onClick={handleSendResultConfirm}>
                    Gửi
                </Button>
            </Modal.Footer>
        </Modal>
    )
}

export default SendEmailPopup
