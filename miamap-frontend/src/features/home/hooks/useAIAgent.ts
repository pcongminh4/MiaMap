"use client"

import { useState, useRef, useEffect } from 'react'
import { useMutation } from '@tanstack/react-query'
import { http } from '../../../services/api/http'
import type { BoundingBoxPlaceResponse } from '../../../services/map/dto/map.dto.response'

import type { Message, UseAIAgentProps, ChatPayload, ChatResponse, ImageSearchResponse } from '../types/home.types'

export function useAIAgent({ mapCenter, onSelectPlace, onDrawRoute }: UseAIAgentProps) {
  const [isOpen, setIsOpen] = useState(false)
  const [messages, setMessages] = useState<Message[]>([
    {
      id: 'welcome',
      sender: 'system',
      text: 'Xin chào! Tôi là Trợ lý Bản đồ MiaMap AI. Hãy gõ câu hỏi để tìm kiếm quán ăn/sản phẩm hoặc tải ảnh lên để tôi nhận diện địa điểm trong Quận 1 nhé! 👋',
    },
  ])
  const [inputValue, setInputValue] = useState('')
  const fileInputRef = useRef<HTMLInputElement>(null)
  const messagesEndRef = useRef<HTMLDivElement>(null)

  // Tự động cuộn xuống cuối tin nhắn
  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' })
  }, [messages])

  // Mutation gửi tin nhắn văn bản (Chat)
  const chatMutation = useMutation<ChatResponse, Error, ChatPayload>({
    mutationFn: async (payload) => {
      const response = await http.post<ChatResponse>('/agent/chat', payload)
      return response.data
    },
    onSuccess: (data) => {
      const recommendedDetails = data.mapActionPayload ?? []
      const botMsgId = (Date.now() + 1).toString()

      setMessages((prev) => [
        ...prev,
        {
          id: botMsgId,
          sender: 'bot',
          text: data.answer,
          places: recommendedDetails,
        },
      ])

      // Xử lý Map Actions từ AI trả về
      if (recommendedDetails.length > 0) {
        if (data.mapAction === 'draw_route') {
          onDrawRoute(recommendedDetails[0])
        } else if (data.mapAction === 'zoom_to_places') {
          onSelectPlace(recommendedDetails[0])
        }
      }
    },
    onError: (error) => {
      setMessages((prev) => [
        ...prev,
        {
          id: Date.now().toString(),
          sender: 'system',
          text: `Không thể kết nối AI: ${error.message || 'Lỗi mạng.'}`,
        },
      ])
    },
  })

  // Mutation tải ảnh nhận diện địa điểm (Image Search)
  const imageSearchMutation = useMutation<ImageSearchResponse, Error, FormData>({
    mutationFn: async (formData) => {
      const response = await http.post<ImageSearchResponse>(
        `/agent/search-image?lat=${mapCenter[0]}&lng=${mapCenter[1]}`,
        formData,
        {
          headers: {
            'Content-Type': 'multipart/form-data',
          },
        }
      )
      return response.data
    },
    onSuccess: (data) => {
      const botMsgId = (Date.now() + 1).toString()

      if (data.success && data.recognizedPlaceId) {
        const matchedPlace: BoundingBoxPlaceResponse = {
          placeId: data.recognizedPlaceId,
          name: data.recognizedName || 'Địa điểm',
          category: data.category || 'cafe',
          address: '',
          location: {
            latitude: data.latitude || mapCenter[0],
            longitude: data.longitude || mapCenter[1],
          },
          rating: 4.5,
          reviewCount: 10,
        }

        setMessages((prev) => [
          ...prev,
          {
            id: botMsgId,
            sender: 'bot',
            text: data.message,
            places: [matchedPlace],
          },
        ])

        // Tự động điều khiển bản đồ di chuyển đến vị trí nhận dạng
        onSelectPlace(matchedPlace)
      } else {
        setMessages((prev) => [
          ...prev,
          {
            id: botMsgId,
            sender: 'bot',
            text: data.message || 'Tôi đã xem ảnh nhưng chưa nhận dạng được địa điểm cụ thể nào trong Quận 1.',
          },
        ])
      }
    },
    onError: (error) => {
      setMessages((prev) => [
        ...prev,
        {
          id: Date.now().toString(),
          sender: 'system',
          text: `Lỗi khi tải ảnh lên: ${error.message || 'Lỗi mạng.'}`,
        },
      ])
    },
  })

  const handleSendText = () => {
    if (!inputValue.trim() || chatMutation.isPending) return

    const userText = inputValue.trim()
    setInputValue('')

    // Thêm tin nhắn của user vào UI trước khi call API
    const userMsgId = Date.now().toString()
    setMessages((prev) => [...prev, { id: userMsgId, sender: 'user', text: userText }])

    // Thực hiện Mutation
    chatMutation.mutate({
      prompt: userText,
      latitude: mapCenter[0],
      longitude: mapCenter[1],
    })
  }

  const handleImageUpload = (event: React.ChangeEvent<HTMLInputElement>) => {
    const files = event.target.files
    if (!files || files.length === 0) return

    const file = files[0]

    // Tạo URL xem trước ảnh locally
    const localImageUrl = URL.createObjectURL(file)
    const userMsgId = Date.now().toString()
    setMessages((prev) => [
      ...prev,
      {
        id: userMsgId,
        sender: 'user',
        text: 'Đang gửi ảnh phân tích...',
        imageUrl: localImageUrl,
      },
    ])

    // Upload lên backend qua FormData
    const formData = new FormData()
    formData.append('file', file)

    // Thực hiện Mutation
    imageSearchMutation.mutate(formData)

    // Clear input file
    if (fileInputRef.current) {
      fileInputRef.current.value = ''
    }
  }

  const isSending = chatMutation.isPending || imageSearchMutation.isPending

  return {
    isOpen,
    setIsOpen,
    messages,
    inputValue,
    setInputValue,
    isSending,
    fileInputRef,
    messagesEndRef,
    handleSendText,
    handleImageUpload,
  }
}
