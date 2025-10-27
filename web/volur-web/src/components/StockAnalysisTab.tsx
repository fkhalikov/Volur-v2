import { useState, useEffect } from 'react'
import { api } from '../api/client'
import { StockNoteDto, StockKeyValueDto } from '../types/api'
import { PencilIcon, TrashIcon, PlusIcon } from '@heroicons/react/24/outline'

interface StockAnalysisTabProps {
  ticker: string
  exchangeCode: string
}

export default function StockAnalysisTab({ ticker, exchangeCode }: StockAnalysisTabProps) {
  const [notes, setNotes] = useState<StockNoteDto[]>([])
  const [keyValues, setKeyValues] = useState<StockKeyValueDto[]>([])
  const [isLoading, setIsLoading] = useState(false)
  
  // Note editing state
  const [editingNoteId, setEditingNoteId] = useState<number | null>(null)
  const [newNoteContent, setNewNoteContent] = useState('')
  const [editingNoteContent, setEditingNoteContent] = useState('')
  
  // KeyValue editing state
  const [editingKeyValueId, setEditingKeyValueId] = useState<number | null>(null)
  const [newKeyValue, setNewKeyValue] = useState<{ key: string; value: string } | null>(null)
  const [editingKeyValue, setEditingKeyValue] = useState({ key: '', value: '' })

  useEffect(() => {
    loadData()
  }, [ticker, exchangeCode])

  const loadData = async () => {
    setIsLoading(true)
    try {
      const [notesData, keyValuesData] = await Promise.all([
        api.getStockNotes(ticker, exchangeCode),
        api.getStockKeyValues(ticker, exchangeCode)
      ])
      setNotes(notesData)
      setKeyValues(keyValuesData)
    } catch (error) {
      console.error('Failed to load analysis data:', error)
    } finally {
      setIsLoading(false)
    }
  }

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    })
  }

  // Note handlers
  const handleCreateNote = async () => {
    if (!newNoteContent.trim()) return
    try {
      const created = await api.createStockNote(ticker, exchangeCode, newNoteContent)
      setNotes([created, ...notes])
      setNewNoteContent('')
    } catch (error) {
      console.error('Failed to create note:', error)
    }
  }

  const handleEditNote = async (note: StockNoteDto) => {
    try {
      const updated = await api.updateStockNote(note.id, editingNoteContent)
      setNotes(notes.map(n => n.id === note.id ? updated : n))
      setEditingNoteId(null)
      setEditingNoteContent('')
    } catch (error) {
      console.error('Failed to update note:', error)
    }
  }

  const handleDeleteNote = async (id: number) => {
    if (!confirm('Are you sure you want to delete this note?')) return
    try {
      await api.deleteStockNote(id)
      setNotes(notes.filter(n => n.id !== id))
    } catch (error) {
      console.error('Failed to delete note:', error)
    }
  }

  // KeyValue handlers
  const handleCreateKeyValue = async () => {
    if (!newKeyValue || !newKeyValue.key.trim() || !newKeyValue.value.trim()) return
    try {
      const created = await api.createStockKeyValue(ticker, exchangeCode, newKeyValue.key, newKeyValue.value)
      setKeyValues([created, ...keyValues])
      setNewKeyValue(null)
    } catch (error) {
      console.error('Failed to create key-value:', error)
    }
  }

  const handleEditKeyValue = async (keyValue: StockKeyValueDto) => {
    try {
      const updated = await api.updateStockKeyValue(keyValue.id, editingKeyValue.key, editingKeyValue.value)
      setKeyValues(keyValues.map(kv => kv.id === keyValue.id ? updated : kv))
      setEditingKeyValueId(null)
      setEditingKeyValue({ key: '', value: '' })
    } catch (error) {
      console.error('Failed to update key-value:', error)
    }
  }

  const handleDeleteKeyValue = async (id: number) => {
    if (!confirm('Are you sure you want to delete this key-value?')) return
    try {
      await api.deleteStockKeyValue(id)
      setKeyValues(keyValues.filter(kv => kv.id !== id))
    } catch (error) {
      console.error('Failed to delete key-value:', error)
    }
  }

  if (isLoading) {
    return <div className="text-center py-8 text-slate-400">Loading...</div>
  }

  return (
    <div className="grid grid-cols-2 gap-6 h-[600px]">
      {/* Left side - Notes */}
      <div className="flex flex-col border-r border-slate-600 pr-6">
        <div className="flex justify-between items-center mb-4">
          <h4 className="text-lg font-semibold text-white">Notes</h4>
          {!newNoteContent && (
            <button
              onClick={() => setNewNoteContent(' ')}
              className="text-xs text-slate-400 hover:text-white"
            >
              + New Note
            </button>
          )}
        </div>

        {/* New note input */}
        {!newNoteContent && (
          <button
            onClick={() => setNewNoteContent(' ')}
            className="mb-4 p-4 border-2 border-dashed border-slate-600 rounded-lg text-slate-400 hover:border-slate-500 hover:text-white transition-colors text-left"
          >
            <PlusIcon className="h-5 w-5 inline mr-2" />
            Add a new note...
          </button>
        )}

        {newNoteContent && (
          <div className="mb-4 p-4 bg-slate-700 rounded-lg">
            <textarea
              value={newNoteContent}
              onChange={(e) => setNewNoteContent(e.target.value)}
              placeholder="Enter your note..."
              className="w-full bg-slate-800 text-white rounded p-2 mb-2 resize-none"
              rows={4}
              autoFocus
            />
            <div className="flex gap-2">
              <button
                onClick={handleCreateNote}
                className="px-3 py-1 bg-blue-600 hover:bg-blue-700 text-white rounded text-sm"
              >
                Save
              </button>
              <button
                onClick={() => setNewNoteContent('')}
                className="px-3 py-1 bg-slate-600 hover:bg-slate-500 text-white rounded text-sm"
              >
                Cancel
              </button>
            </div>
          </div>
        )}

        {/* Notes list */}
        <div className="flex-1 overflow-y-auto space-y-3">
          {notes.map((note) => (
            <div key={note.id} className="bg-slate-700 rounded-lg p-4">
              {editingNoteId === note.id ? (
                <div>
                  <textarea
                    value={editingNoteContent}
                    onChange={(e) => setEditingNoteContent(e.target.value)}
                    className="w-full bg-slate-800 text-white rounded p-2 mb-2 resize-none"
                    rows={4}
                    autoFocus
                  />
                  <div className="flex gap-2">
                    <button
                      onClick={() => handleEditNote(note)}
                      className="px-3 py-1 bg-blue-600 hover:bg-blue-700 text-white rounded text-sm"
                    >
                      Save
                    </button>
                    <button
                      onClick={() => {
                        setEditingNoteId(null)
                        setEditingNoteContent('')
                      }}
                      className="px-3 py-1 bg-slate-600 hover:bg-slate-500 text-white rounded text-sm"
                    >
                      Cancel
                    </button>
                  </div>
                </div>
              ) : (
                <>
                  <p className="text-white mb-2 whitespace-pre-wrap">{note.content}</p>
                  <div className="flex justify-between items-center">
                    <span className="text-xs text-slate-400">{formatDate(note.createdAt)}</span>
                    <div className="flex gap-2">
                      <button
                        onClick={() => {
                          setEditingNoteId(note.id)
                          setEditingNoteContent(note.content)
                        }}
                        className="text-slate-400 hover:text-white"
                      >
                        <PencilIcon className="h-4 w-4" />
                      </button>
                      <button
                        onClick={() => handleDeleteNote(note.id)}
                        className="text-slate-400 hover:text-red-400"
                      >
                        <TrashIcon className="h-4 w-4" />
                      </button>
                    </div>
                  </div>
                </>
              )}
            </div>
          ))}
          {notes.length === 0 && (
            <p className="text-center text-slate-500 text-sm py-8">
              No notes yet. Create one to get started!
            </p>
          )}
        </div>
      </div>

      {/* Right side - Key-Values */}
      <div className="flex flex-col">
        <div className="flex justify-between items-center mb-4">
          <h4 className="text-lg font-semibold text-white">Key-Value Pairs</h4>
        </div>

        {/* New key-value input */}
        {newKeyValue !== null ? (
          <div className="mb-4 p-4 bg-slate-700 rounded-lg">
            <input
              type="text"
              value={newKeyValue.key}
              onChange={(e) => setNewKeyValue({ ...newKeyValue, key: e.target.value })}
              placeholder="Key"
              className="w-full bg-slate-800 text-white rounded p-2 mb-2"
              autoFocus
            />
            <input
              type="text"
              value={newKeyValue.value}
              onChange={(e) => setNewKeyValue({ ...newKeyValue, value: e.target.value })}
              placeholder="Value"
              className="w-full bg-slate-800 text-white rounded p-2 mb-2"
            />
            <div className="flex gap-2">
              <button
                onClick={handleCreateKeyValue}
                className="px-3 py-1 bg-blue-600 hover:bg-blue-700 text-white rounded text-sm"
              >
                Save
              </button>
              <button
                onClick={() => setNewKeyValue(null)}
                className="px-3 py-1 bg-slate-600 hover:bg-slate-500 text-white rounded text-sm"
              >
                Cancel
              </button>
            </div>
          </div>
        ) : (
          <button
            onClick={() => setNewKeyValue({ key: '', value: '' })}
            className="mb-4 p-4 border-2 border-dashed border-slate-600 rounded-lg text-slate-400 hover:border-slate-500 hover:text-white transition-colors text-left"
          >
            <PlusIcon className="h-5 w-5 inline mr-2" />
            Add a new key-value pair...
          </button>
        )}

        {/* Key-value list */}
        <div className="flex-1 overflow-y-auto space-y-3">
          {keyValues.map((kv) => (
            <div key={kv.id} className="bg-slate-700 rounded-lg p-4">
              {editingKeyValueId === kv.id ? (
                <div>
                  <input
                    type="text"
                    value={editingKeyValue.key}
                    onChange={(e) => setEditingKeyValue({ ...editingKeyValue, key: e.target.value })}
                    className="w-full bg-slate-800 text-white rounded p-2 mb-2"
                    autoFocus
                  />
                  <input
                    type="text"
                    value={editingKeyValue.value}
                    onChange={(e) => setEditingKeyValue({ ...editingKeyValue, value: e.target.value })}
                    className="w-full bg-slate-800 text-white rounded p-2 mb-2"
                  />
                  <div className="flex gap-2">
                    <button
                      onClick={() => handleEditKeyValue(kv)}
                      className="px-3 py-1 bg-blue-600 hover:bg-blue-700 text-white rounded text-sm"
                    >
                      Save
                    </button>
                    <button
                      onClick={() => {
                        setEditingKeyValueId(null)
                        setEditingKeyValue({ key: '', value: '' })
                      }}
                      className="px-3 py-1 bg-slate-600 hover:bg-slate-500 text-white rounded text-sm"
                    >
                      Cancel
                    </button>
                  </div>
                </div>
              ) : (
                <>
                  <div className="flex justify-between items-start mb-1">
                    <span className="text-white font-medium">{kv.key}</span>
                    <div className="flex gap-2">
                      <button
                        onClick={() => {
                          setEditingKeyValueId(kv.id)
                          setEditingKeyValue({ key: kv.key, value: kv.value })
                        }}
                        className="text-slate-400 hover:text-white"
                      >
                        <PencilIcon className="h-4 w-4" />
                      </button>
                      <button
                        onClick={() => handleDeleteKeyValue(kv.id)}
                        className="text-slate-400 hover:text-red-400"
                      >
                        <TrashIcon className="h-4 w-4" />
                      </button>
                    </div>
                  </div>
                  <p className="text-slate-300 text-sm mb-2">{kv.value}</p>
                  <span className="text-xs text-slate-400">{formatDate(kv.createdAt)}</span>
                </>
              )}
            </div>
          ))}
          {keyValues.length === 0 && (
            <p className="text-center text-slate-500 text-sm py-8">
              No key-value pairs yet. Create one to get started!
            </p>
          )}
        </div>
      </div>
    </div>
  )
}
