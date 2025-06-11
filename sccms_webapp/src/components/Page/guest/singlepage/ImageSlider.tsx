import React, { useState, useEffect } from "react"
import "../../../../assets/css/ImageSlider.css"

interface ImageData {
    src: string
    caption: string
}

interface ImageSliderProps {
    images: ImageData[]
    currentImageIndex: number
    onClose: () => void
}

const ImageSlider: React.FC<ImageSliderProps> = ({ images, currentImageIndex, onClose }) => {
    const [currentIndex, setCurrentIndex] = useState(currentImageIndex)

    // Clean caption by removing <i> tags
    const cleanCaption = (caption: string) => {
        return caption.replace(/<\/?i>/g, "")
    }

    // Navigate to the next image
    const handleNext = () => {
        setCurrentIndex((prevIndex) => (prevIndex + 1 < images.length ? prevIndex + 1 : 0))
    }

    // Navigate to the previous image
    const handlePrev = () => {
        setCurrentIndex((prevIndex) => (prevIndex - 1 >= 0 ? prevIndex - 1 : images.length - 1))
    }

    const currentImage = images[currentIndex]

    return (
        <div className="slider-overlay">
            <button className="slider-close" onClick={onClose}>
                ✕
            </button>

            {/* Image count at the top */}
            <div className="slider-image-count">
                Image {currentIndex + 1} / {images.length}
            </div>

            <div className="slider-container">
                <button className="slider-nav left" onClick={handlePrev}>
                    <svg width="24" height="24" viewBox="0 0 24 24" fill="none">
                        <path
                            d="M15 6L9 12L15 18"
                            stroke="currentColor"
                            strokeWidth="2"
                            strokeLinecap="round"
                            strokeLinejoin="round"
                        />
                    </svg>
                </button>
                <img src={currentImage.src} alt={`Slide ${currentIndex + 1}`} className="slider-image" />
                <button className="slider-nav right" onClick={handleNext}>
                    <svg width="24" height="24" viewBox="0 0 24 24" fill="none">
                        <path
                            d="M9 6L15 12L9 18"
                            stroke="currentColor"
                            strokeWidth="2"
                            strokeLinecap="round"
                            strokeLinejoin="round"
                        />
                    </svg>
                </button>
            </div>

            {/* Display caption without <i> tags */}
            {currentImage.caption && <div className="slider-caption">{cleanCaption(currentImage.caption)}</div>}
        </div>
    )
}

export default ImageSlider
