
export type SortDirection = 'asc' | 'desc' | null

export interface SortableHeaderProps {
  title: string
  sortKey: string
  currentSortBy?: string
  currentSortDirection?: SortDirection
  onSort: (sortBy: string, direction: SortDirection) => void
  className?: string
  align?: 'left' | 'right' | 'center'
}

export default function SortableHeader({
  title,
  sortKey,
  currentSortBy,
  currentSortDirection,
  onSort,
  className = '',
  align = 'left'
}: SortableHeaderProps) {
  const isActive = currentSortBy === sortKey
  const isAscending = isActive && currentSortDirection === 'asc'
  const isDescending = isActive && currentSortDirection === 'desc'

  const handleClick = () => {
    let newDirection: SortDirection = 'asc'
    
    if (isActive) {
      if (isAscending) {
        newDirection = 'desc'
      } else if (isDescending) {
        newDirection = null
      } else {
        newDirection = 'asc'
      }
    }
    
    onSort(sortKey, newDirection)
  }

  const getAlignmentClass = () => {
    switch (align) {
      case 'right':
        return 'text-right'
      case 'center':
        return 'text-center'
      default:
        return 'text-left'
    }
  }

  const getSortIcon = () => {
    if (isAscending) {
      return (
        <svg className="w-4 h-4 ml-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 15l7-7 7 7" />
        </svg>
      )
    }
    
    if (isDescending) {
      return (
        <svg className="w-4 h-4 ml-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
        </svg>
      )
    }
    
    return (
      <svg className="w-4 h-4 ml-1 opacity-30" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 9l4-4 4 4m0 6l-4 4-4-4" />
      </svg>
    )
  }

  return (
    <th
      className={`px-6 py-3 text-xs font-medium text-slate-300 uppercase tracking-wider cursor-pointer hover:bg-slate-600 transition-colors select-none ${getAlignmentClass()} ${className}`}
      onClick={handleClick}
      title={`Sort by ${title}${isActive ? ` (currently ${currentSortDirection})` : ''}`}
    >
      <div className="flex items-center">
        <span>{title}</span>
        {getSortIcon()}
      </div>
    </th>
  )
}
